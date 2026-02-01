using MediatR;

namespace PizzaStore.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(string CartItemId) : IRequest<RemoveCartItemResponse>;
