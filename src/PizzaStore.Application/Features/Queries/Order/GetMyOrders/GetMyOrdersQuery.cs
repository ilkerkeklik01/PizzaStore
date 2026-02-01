using MediatR;

namespace PizzaStore.Application.Features.Queries.Order.GetMyOrders;

public record GetMyOrdersQuery : IRequest<List<OrderDto>>;
