using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

namespace PizzaStore.Application.Features.Cart.Commands.DecreaseCartItemQuantity;

public record DecreaseCartItemQuantityCommand(string CartItemId, int Amount) : IRequest<CartItemDto>, ISecuredRequest;
