using FluentValidation;
using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;

/// <summary>
/// Handles updates to existing pizza details by administrators
/// </summary>
public class UpdatePizzaCommandHandler : IRequestHandler<UpdatePizzaCommand, UpdatePizzaResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdatePizzaDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePizzaCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdatePizzaDto> validator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatePizzaResponse> Handle(UpdatePizzaCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can update pizzas");

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.UpdatePizzaDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errorMessage);
        }

        // Find the pizza
        var pizza = await _unitOfWork.Pizzas.GetByIdAsync(request.Id);
        
        if (pizza == null)
        {
            throw new NotFoundException($"Pizza with ID '{request.Id}' not found.");
        }

        // Update properties
        pizza.Name = request.UpdatePizzaDto.Name;
        pizza.Description = request.UpdatePizzaDto.Description;
        pizza.Type = request.UpdatePizzaDto.Type;
        pizza.ImageUrl = request.UpdatePizzaDto.ImageUrl;
        pizza.IsAvailable = request.UpdatePizzaDto.IsAvailable;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePizzaResponse
        {
            Id = pizza.Id,
            Name = pizza.Name,
            Message = $"Pizza '{pizza.Name}' has been updated successfully."
        };
    }
}
