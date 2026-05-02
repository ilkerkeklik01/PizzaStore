using MediatR;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllOrders;

/// <summary>
/// Retrieves all orders with optional filtering by status, user, and date range
/// </summary>
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        // DB-level filtering by status and userId; date range filtered in-memory below
        var orders = await _unitOfWork.Orders.GetAllOrdersWithDetailsAsync(request.Status, request.UserId);

        if (request.FromDate.HasValue)
            orders = orders.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
        {
            var toDateEnd = request.ToDate.Value.AddDays(1);
            orders = orders.Where(o => o.CreatedAt < toDateEnd);
        }

        return orders
            .Select(OrderDto.FromEntity)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }
}
