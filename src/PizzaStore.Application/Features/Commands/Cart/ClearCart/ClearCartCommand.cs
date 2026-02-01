using MediatR;

namespace PizzaStore.Application.Features.Commands.Cart.ClearCart;

public record ClearCartCommand : IRequest<ClearCartResponse>;
