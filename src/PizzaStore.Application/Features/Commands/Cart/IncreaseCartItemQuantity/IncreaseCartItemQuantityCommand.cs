using MediatR;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

namespace PizzaStore.Application.Features.Commands.Cart.IncreaseCartItemQuantity;

public record IncreaseCartItemQuantityCommand(string CartItemId, int Amount) : IRequest<CartItemDto>;
