using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Pizza.CreatePizza;

/// <summary>
/// Handles creation of new pizzas with variants by administrators
/// </summary>
public class CreatePizzaCommandHandler : IRequestHandler<CreatePizzaCommand, CreatePizzaResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePizzaDto> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreatePizzaCommandHandler> _logger;

    public CreatePizzaCommandHandler(IUnitOfWork unitOfWork, IValidator<CreatePizzaDto> validator, ICurrentUserService currentUserService, ILogger<CreatePizzaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreatePizzaResponse> Handle(CreatePizzaCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can create pizzas");

        _logger.LogInformation("Admin creating new pizza: {PizzaName}", request.CreatePizzaDto.Name);

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.CreatePizzaDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Pizza creation validation failed: {ValidationErrors}", errorMessage);
            throw new Core.CrossCuttingConcerns.Exceptions.ValidationException(errorMessage);
        }

        // Create the pizza entity
        var pizza = new PizzaStore.Domain.Entities.Pizza
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.CreatePizzaDto.Name,
            Description = request.CreatePizzaDto.Description,
            Type = request.CreatePizzaDto.Type,
            ImageUrl = request.CreatePizzaDto.ImageUrl,
            IsAvailable = true
        };

        // Create pizza variants
        foreach (var variantDto in request.CreatePizzaDto.Variants)
        {
            var variant = new PizzaStore.Domain.Entities.PizzaVariant
            {
                Id = Guid.NewGuid().ToString(),
                PizzaId = pizza.Id,
                Size = variantDto.Size,
                Price = variantDto.Price,
                IsAvailable = true
            };
            pizza.Variants.Add(variant);
        }

        // Save to database
        await _unitOfWork.Pizzas.AddAsync(pizza);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pizza {PizzaId} '{PizzaName}' created successfully with {VariantCount} variants", 
            pizza.Id, pizza.Name, pizza.Variants.Count);

        return new CreatePizzaResponse
        {
            Id = pizza.Id,
            Name = pizza.Name,
            Message = $"Pizza '{pizza.Name}' has been created successfully with {pizza.Variants.Count} variant(s)."
        };
    }
}
