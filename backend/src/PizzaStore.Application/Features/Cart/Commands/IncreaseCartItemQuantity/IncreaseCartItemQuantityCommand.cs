using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

namespace PizzaStore.Application.Features.Cart.Commands.IncreaseCartItemQuantity;

public record IncreaseCartItemQuantityCommand(string CartItemId, int Amount) : IRequest<CartItemDto>, ISecuredRequest;
