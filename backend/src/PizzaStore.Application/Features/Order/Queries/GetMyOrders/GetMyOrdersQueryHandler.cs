using MediatR;
using PizzaStore.Application.Common.Models;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Order.Queries.GetMyOrders;

/// <summary>
/// Retrieves a filtered, paginated page of orders for the authenticated user
/// </summary>
public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetAuthenticatedUserId();

        var (orders, totalCount) = await _unitOfWork.Orders.GetFilteredOrdersByUserIdAsync(
            userId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize);

        var items = orders
            .Select(OrderDto.FromEntity)
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResult<OrderDto>(items, totalCount, request.Page, request.PageSize, totalPages);
    }
}
