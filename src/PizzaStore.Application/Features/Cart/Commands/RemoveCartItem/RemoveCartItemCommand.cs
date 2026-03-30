using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(string CartItemId) : IRequest<RemoveCartItemResponse>, ISecuredRequest;
