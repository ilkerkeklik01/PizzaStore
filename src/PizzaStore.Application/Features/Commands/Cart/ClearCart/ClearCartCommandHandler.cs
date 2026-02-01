using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.ClearCart;

/// <summary>
/// Handles clearing all items from the user's cart
/// </summary>
public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, ClearCartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ClearCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ClearCartResponse> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Get user's cart
        var cart = await _unitOfWork.Carts.GetCartWithItemsByUserIdAsync(userId);
        if (cart == null)
        {
            return new ClearCartResponse
            {
                Message = "Cart is already empty",
                Success = true,
                ItemsRemoved = 0
            };
        }

        // Delete all cart items
        var itemsRemoved = cart.CartItems.Count;
        var itemsToDelete = cart.CartItems.ToList();
        foreach (var cartItem in itemsToDelete)
        {
            await _unitOfWork.CartItems.DeleteAsync(cartItem);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClearCartResponse
        {
            Message = $"Cart cleared successfully. {itemsRemoved} item(s) removed.",
            Success = true,
            ItemsRemoved = itemsRemoved
        };
    }
}
