using MediatR;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Admin.GetAllOrders;

public record GetAllOrdersQuery(
    OrderStatus? Status = null,
    string? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<List<OrderDto>>;
