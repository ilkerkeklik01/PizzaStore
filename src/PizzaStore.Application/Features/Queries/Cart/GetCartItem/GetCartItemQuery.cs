using MediatR;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

namespace PizzaStore.Application.Features.Queries.Cart.GetCartItem;

public record GetCartItemQuery(string CartItemId) : IRequest<CartItemDto>;
