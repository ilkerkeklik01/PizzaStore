using MediatR;

namespace PizzaStore.Application.Features.Order.Queries.GetOrderById;

public record GetOrderByIdQuery(string OrderId) : IRequest<OrderDto>;
