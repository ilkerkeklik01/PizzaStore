using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.AddPizzaVariant;

public class AddPizzaVariantCommandHandler : IRequestHandler<AddPizzaVariantCommand, AddPizzaVariantResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddPizzaVariantDto> _validator;

    public AddPizzaVariantCommandHandler(IUnitOfWork unitOfWork, IValidator<AddPizzaVariantDto> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AddPizzaVariantResponse> Handle(AddPizzaVariantCommand request, CancellationToken cancellationToken)
    {
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
        var existingVariant = await _unitOfWork.PizzaVariants
            .GetByPizzaIdAndSizeAsync(request.AddPizzaVariantDto.PizzaId, request.AddPizzaVariantDto.Size);

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
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Unique constraint on (PizzaId, Size) can still be hit in races
            throw new ValidationException(
                $"Pizza variant with size '{request.AddPizzaVariantDto.Size}' already exists for this pizza.");
        }

        return new AddPizzaVariantResponse
        {
            Id = variant.Id,
            Size = variant.Size.ToString(),
            Price = variant.Price,
            Message = $"Pizza variant (Size: {variant.Size}) has been added successfully to pizza '{pizza.Name}'."
        };
    }
}
