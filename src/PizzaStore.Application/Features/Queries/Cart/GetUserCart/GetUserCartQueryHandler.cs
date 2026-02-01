using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Cart.GetUserCart;

/// <summary>
/// Retrieves the authenticated user's shopping cart with all items
/// </summary>
public class GetUserCartQueryHandler : IRequestHandler<GetUserCartQuery, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserCartQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CartDto> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
    {
        // Check authentication
        var currentUserId = _currentUserService.GetAuthenticatedUserId();

        // Verify that user is requesting their own cart
        if (request.UserId != currentUserId)
            throw new UnauthorizedException("You do not have permission to view this cart");

        // Get user's cart with items
        var cart = await _unitOfWork.Carts.GetCartWithItemsByUserIdAsync(request.UserId);

        if (cart == null)
        {
            // Return empty cart DTO
            return new CartDto
            {
                Id = string.Empty,
                UserId = request.UserId,
                Items = new()
            };
        }

        // Map cart items to DTOs
        var cartItemDtos = cart.CartItems
            .Select(cartItem => CartItemDto.FromEntity(cartItem))
            .ToList();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cartItemDtos
        };
    }
}
