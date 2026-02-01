using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.IncreaseCartItemQuantity;

public class IncreaseCartItemQuantityCommandHandler : IRequestHandler<IncreaseCartItemQuantityCommand, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public IncreaseCartItemQuantityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CartItemDto> Handle(IncreaseCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Validate amount
        if (request.Amount <= 0)
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException("Amount must be greater than 0");

        // Verify cart item exists and belongs to user
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item with ID '{request.CartItemId}' not found");

        var isOwned = await _unitOfWork.Carts.IsCartItemOwnedByUserAsync(request.CartItemId, userId);
        if (!isOwned)
            throw new UnauthorizedException("You do not have permission to update this cart item");

        // Increase quantity
        cartItem.Quantity += request.Amount;

        // Save changes - EF Core tracks changes automatically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload cart item with details
        var updatedCartItem = await _unitOfWork.CartItems.GetCartItemWithDetailsAsync(cartItem.Id);
        if (updatedCartItem == null)
            throw new NotFoundException($"Cart item with ID '{cartItem.Id}' not found after update");

        return CartItemDto.FromEntity(updatedCartItem);
    }
}
