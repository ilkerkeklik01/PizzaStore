using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

namespace PizzaStore.Application.Features.Cart.Queries.GetCartItem;

public record GetCartItemQuery(string CartItemId) : IRequest<CartItemDto>, ISecuredRequest;
