using MediatR;

namespace PizzaStore.Application.Features.Queries.Cart.GetUserCart;

public record GetUserCartQuery(string UserId) : IRequest<CartDto>;
