using MediatR;

namespace PizzaStore.Application.Features.Queries.Order.GetOrderById;

public record GetOrderByIdQuery(string OrderId) : IRequest<OrderDto>;
