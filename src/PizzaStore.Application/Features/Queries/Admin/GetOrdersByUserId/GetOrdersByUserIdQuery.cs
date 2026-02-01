using MediatR;
using PizzaStore.Application.Features.Queries.Order;

namespace PizzaStore.Application.Features.Queries.Admin.GetOrdersByUserId;

public record GetOrdersByUserIdQuery(string UserId) : IRequest<List<OrderDto>>;
