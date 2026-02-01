using MediatR;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllOrders;

public record GetAllOrdersQuery(
    OrderStatus? Status = null,
    string? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<List<OrderDto>>;
