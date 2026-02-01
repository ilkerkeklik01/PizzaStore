using MediatR;

namespace PizzaStore.Application.Features.Cart.Queries.GetUserCart;

public record GetUserCartQuery(string UserId) : IRequest<CartDto>;
