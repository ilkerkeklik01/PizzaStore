using MediatR;

namespace PizzaStore.Application.Features.Commands.Cart.RemoveCartItem;

public record RemoveCartItemCommand(string CartItemId) : IRequest<RemoveCartItemResponse>;
