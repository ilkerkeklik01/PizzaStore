using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;

public record AddPizzaToCartCommand(AddPizzaToCartDto Dto) : IRequest<CartItemDto>, ISecuredRequest;
