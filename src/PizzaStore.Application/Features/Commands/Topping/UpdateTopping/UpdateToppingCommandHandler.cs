using FluentValidation;
using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Commands.Topping.UpdateTopping;

public class UpdateToppingCommandHandler : IRequestHandler<UpdateToppingCommand, UpdateToppingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateToppingDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public UpdateToppingCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateToppingDto> validator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateToppingResponse> Handle(UpdateToppingCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can update toppings");

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.UpdateToppingDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errorMessage);
        }

        // Find the topping
        var topping = await _unitOfWork.Toppings.GetByIdAsync(request.Id);
        
        if (topping == null)
        {
            throw new NotFoundException($"Topping with ID '{request.Id}' not found.");
        }

        // Update properties
        topping.Name = request.UpdateToppingDto.Name;
        topping.Price = request.UpdateToppingDto.Price;
        topping.IsAvailable = request.UpdateToppingDto.IsAvailable;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateToppingResponse
        {
            Id = topping.Id,
            Name = topping.Name,
            Price = topping.Price,
            IsAvailable = topping.IsAvailable,
            Message = $"Topping '{topping.Name}' has been updated successfully."
        };
    }
}
