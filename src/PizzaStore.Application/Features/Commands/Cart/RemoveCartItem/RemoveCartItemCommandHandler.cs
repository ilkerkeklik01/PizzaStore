using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.RemoveCartItem;

/// <summary>
/// Handles removal of a specific item from the user's cart
/// </summary>
public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, RemoveCartItemResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<RemoveCartItemResponse> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Verify cart item exists
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item with ID '{request.CartItemId}' not found");

        // Verify ownership
        var isOwned = await _unitOfWork.Carts.IsCartItemOwnedByUserAsync(request.CartItemId, userId);
        if (!isOwned)
            throw new UnauthorizedException("You do not have permission to remove this cart item");

        // Delete cart item (cascades to CartItemToppings)
        await _unitOfWork.CartItems.DeleteAsync(cartItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RemoveCartItemResponse
        {
            Message = "Cart item removed successfully",
            Success = true
        };
    }
}
