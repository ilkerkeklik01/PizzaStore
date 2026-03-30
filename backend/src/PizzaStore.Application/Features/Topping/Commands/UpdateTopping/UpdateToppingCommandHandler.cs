using FluentValidation;
using MediatR;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Topping.Commands.UpdateTopping;

public class UpdateToppingCommandHandler : IRequestHandler<UpdateToppingCommand, UpdateToppingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateToppingDto> _validator;

    public UpdateToppingCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateToppingDto> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<UpdateToppingResponse> Handle(UpdateToppingCommand request, CancellationToken cancellationToken)
    {
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
