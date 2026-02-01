using MediatR;

namespace PizzaStore.Application.Features.Order.Queries.GetMyOrders;

public record GetMyOrdersQuery : IRequest<List<OrderDto>>;
