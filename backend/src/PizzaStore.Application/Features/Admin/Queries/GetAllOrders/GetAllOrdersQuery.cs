using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Common.Models;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllOrders;

public record GetAllOrdersQuery(
    OrderStatus? Status = null,
    string? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderDto>>, IAdminRequest;
