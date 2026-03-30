using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Cart.Queries.GetUserCart;

public record GetUserCartQuery(string UserId) : IRequest<CartDto>, ISecuredRequest;
