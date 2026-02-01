using FluentValidation;
using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;

public class UpdatePizzaVariantCommandHandler : IRequestHandler<UpdatePizzaVariantCommand, UpdatePizzaVariantResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdatePizzaVariantDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePizzaVariantCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdatePizzaVariantDto> validator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatePizzaVariantResponse> Handle(UpdatePizzaVariantCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can update pizza variants");

        // Validate the DTO
        var validationResult = await _validator.ValidateAsync(request.UpdatePizzaVariantDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errorMessage);
        }

        // Find the pizza variant
        var variant = await _unitOfWork.PizzaVariants.GetByIdAsync(request.Id);
        
        if (variant == null)
        {
            throw new NotFoundException($"Pizza variant with ID '{request.Id}' not found.");
        }

        // Update properties
        variant.Price = request.UpdatePizzaVariantDto.Price;
        variant.IsAvailable = request.UpdatePizzaVariantDto.IsAvailable;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePizzaVariantResponse
        {
            Id = variant.Id,
            Price = variant.Price,
            IsAvailable = variant.IsAvailable,
            Message = $"Pizza variant has been updated successfully."
        };
    }
}
