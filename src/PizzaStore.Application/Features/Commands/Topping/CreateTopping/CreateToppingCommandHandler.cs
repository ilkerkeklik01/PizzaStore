using FluentValidation;
using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Commands.Topping.CreateTopping;

/// <summary>
/// Handles creation of new toppings by administrators
/// </summary>
public class CreateToppingCommandHandler : IRequestHandler<CreateToppingCommand, CreateToppingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateToppingDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public CreateToppingCommandHandler(IUnitOfWork unitOfWork, IValidator<CreateToppingDto> validator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<CreateToppingResponse> Handle(CreateToppingCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can create toppings");

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.CreateToppingDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errorMessage);
        }

        // Create the topping entity
        var topping = new PizzaStore.Domain.Entities.Topping
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.CreateToppingDto.Name,
            Price = request.CreateToppingDto.Price,
            IsAvailable = true
        };

        // Save to database
        await _unitOfWork.Toppings.AddAsync(topping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateToppingResponse
        {
            Id = topping.Id,
            Name = topping.Name,
            Price = topping.Price,
            Message = $"Topping '{topping.Name}' has been created successfully."
        };
    }
}
