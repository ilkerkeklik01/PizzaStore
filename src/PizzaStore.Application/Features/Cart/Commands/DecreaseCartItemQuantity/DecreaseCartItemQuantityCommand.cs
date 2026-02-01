using MediatR;
using PizzaStore.Application.Features.Cart.Commands.RemoveCartItem;

namespace PizzaStore.Application.Features.Cart.Commands.DecreaseCartItemQuantity;

public record DecreaseCartItemQuantityCommand(string CartItemId, int Amount) : IRequest<DecreaseCartItemQuantityResponse>;
