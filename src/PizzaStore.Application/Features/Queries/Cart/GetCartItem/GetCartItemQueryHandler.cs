using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Cart.GetCartItem;

public class GetCartItemQueryHandler : IRequestHandler<GetCartItemQuery, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCartItemQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CartItemDto> Handle(GetCartItemQuery request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Get cart item
        var cartItem = await _unitOfWork.CartItems.GetCartItemWithDetailsAsync(request.CartItemId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item with ID '{request.CartItemId}' not found");

        // Verify ownership
        var isOwned = await _unitOfWork.Carts.IsCartItemOwnedByUserAsync(request.CartItemId, userId);
        if (!isOwned)
            throw new UnauthorizedException("You do not have permission to view this cart item");

        return CartItemDto.FromEntity(cartItem);
    }
}
