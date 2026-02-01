using MediatR;
using PizzaStore.Application.Features.Commands.Cart.RemoveCartItem;

namespace PizzaStore.Application.Features.Commands.Cart.DecreaseCartItemQuantity;

public record DecreaseCartItemQuantityCommand(string CartItemId, int Amount) : IRequest<DecreaseCartItemQuantityResponse>;
