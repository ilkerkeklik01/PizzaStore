using MediatR;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

namespace PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;

public record UpdateCartItemQuantityCommand(UpdateCartItemQuantityDto Dto) : IRequest<CartItemDto>;
