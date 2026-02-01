using MediatR;

namespace PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

public record AddPizzaToCartCommand(AddPizzaToCartDto Dto) : IRequest<CartItemDto>;
