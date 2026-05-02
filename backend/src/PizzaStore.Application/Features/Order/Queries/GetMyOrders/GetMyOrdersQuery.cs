using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Common.Models;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Order.Queries.GetMyOrders;

public record GetMyOrdersQuery(
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderDto>>, ISecuredRequest;
