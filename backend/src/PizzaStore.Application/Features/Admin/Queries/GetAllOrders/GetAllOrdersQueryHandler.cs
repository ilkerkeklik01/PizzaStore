using MediatR;
using PizzaStore.Application.Common.Models;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllOrders;

/// <summary>
/// Retrieves a filtered, paginated page of all orders (admin)
/// </summary>
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _unitOfWork.Orders.GetAllOrdersPagedAsync(
            request.Status,
            request.UserId,
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
