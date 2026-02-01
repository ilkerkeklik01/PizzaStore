using MediatR;

namespace PizzaStore.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand : IRequest<ClearCartResponse>;
