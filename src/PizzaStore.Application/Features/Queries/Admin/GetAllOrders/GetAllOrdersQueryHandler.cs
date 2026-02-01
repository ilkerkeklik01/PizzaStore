using MediatR;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Admin.GetAllOrders;

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
        // Get all orders from repository
        var orders = await _unitOfWork.Orders.GetAllAsync();

        // Apply status filter if provided
        if (request.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value).ToList();
        }

        // Apply user ID filter if provided
        if (!string.IsNullOrEmpty(request.UserId))
        {
            orders = orders.Where(o => o.UserId == request.UserId).ToList();
        }

        // Apply date range filter if provided
        if (request.FromDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt >= request.FromDate.Value).ToList();
        }

        if (request.ToDate.HasValue)
        {
            // Add one day to include the entire ToDate
            var toDateEnd = request.ToDate.Value.AddDays(1);
            orders = orders.Where(o => o.CreatedAt < toDateEnd).ToList();
        }

        // Map to DTOs and order by CreatedAt descending
        return orders
            .Select(order => OrderDto.FromEntity(order))
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }
}
