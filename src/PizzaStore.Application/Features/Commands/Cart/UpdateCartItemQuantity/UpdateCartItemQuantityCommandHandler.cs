using FluentValidation;
using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCartItemQuantityDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCartItemQuantityCommandHandler(
        IUnitOfWork unitOfWork,
        IValidator<UpdateCartItemQuantityDto> validator,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<CartItemDto> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Validate DTO
        var validationResult = await _validator.ValidateAsync(request.Dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException(errorMessage);
        }

        // Verify cart item exists and belongs to user
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.Dto.CartItemId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item with ID '{request.Dto.CartItemId}' not found");

        var isOwned = await _unitOfWork.Carts.IsCartItemOwnedByUserAsync(request.Dto.CartItemId, userId);
        if (!isOwned)
            throw new UnauthorizedException("You do not have permission to update this cart item");

        // Update quantity and special instructions
        cartItem.Quantity = request.Dto.Quantity;
        if (!string.IsNullOrEmpty(request.Dto.SpecialInstructions))
            cartItem.SpecialInstructions = request.Dto.SpecialInstructions;

        // Save changes - EF Core tracks changes automatically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload cart item with details
        var updatedCartItem = await _unitOfWork.CartItems.GetCartItemWithDetailsAsync(cartItem.Id);
        if (updatedCartItem == null)
            throw new NotFoundException($"Cart item with ID '{cartItem.Id}' not found after update");

        return CartItemDto.FromEntity(updatedCartItem);
    }
}
