using MediatR;
using PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

namespace PizzaStore.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public record UpdateCartItemQuantityCommand(UpdateCartItemQuantityDto Dto) : IRequest<CartItemDto>;
