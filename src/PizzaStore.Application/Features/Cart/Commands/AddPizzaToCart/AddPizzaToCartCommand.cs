using MediatR;

namespace PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

public record AddPizzaToCartCommand(AddPizzaToCartDto Dto) : IRequest<CartItemDto>;
