using FluentValidation;
using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.AddPizzaVariant;

public class AddPizzaVariantCommandHandler : IRequestHandler<AddPizzaVariantCommand, AddPizzaVariantResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddPizzaVariantDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public AddPizzaVariantCommandHandler(IUnitOfWork unitOfWork, IValidator<AddPizzaVariantDto> validator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<AddPizzaVariantResponse> Handle(AddPizzaVariantCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can add pizza variants");

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.AddPizzaVariantDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errorMessage);
        }

        // Check if pizza exists
        var pizza = await _unitOfWork.Pizzas.GetByIdAsync(request.AddPizzaVariantDto.PizzaId);
        if (pizza == null)
        {
            throw new NotFoundException($"Pizza with ID '{request.AddPizzaVariantDto.PizzaId}' not found.");
        }

        // Check if variant with same (PizzaId, Size) combination already exists
        var existingVariant = pizza.Variants?
            .FirstOrDefault(v => v.Size == request.AddPizzaVariantDto.Size && v.IsAvailable);
        
        if (existingVariant != null)
        {
            throw new ValidationException(
                $"Pizza variant with size '{request.AddPizzaVariantDto.Size}' already exists for this pizza.");
        }

        // Create the pizza variant entity
        var variant = new PizzaStore.Domain.Entities.PizzaVariant
        {
            Id = Guid.NewGuid().ToString(),
            PizzaId = request.AddPizzaVariantDto.PizzaId,
            Size = request.AddPizzaVariantDto.Size,
            Price = request.AddPizzaVariantDto.Price,
            IsAvailable = true
        };

        // Save to database
        await _unitOfWork.PizzaVariants.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddPizzaVariantResponse
        {
            Id = variant.Id,
            Size = variant.Size.ToString(),
            Price = variant.Price,
            Message = $"Pizza variant (Size: {variant.Size}) has been added successfully to pizza '{pizza.Name}'."
        };
    }
}
