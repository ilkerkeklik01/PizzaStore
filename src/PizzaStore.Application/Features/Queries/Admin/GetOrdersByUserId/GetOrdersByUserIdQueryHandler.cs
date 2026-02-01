using MediatR;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Admin.GetOrdersByUserId;

public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersByUserIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        // Get all orders for the user with full details
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(request.UserId);
        if (orders == null || !orders.Any())
            throw new NotFoundException($"No orders found for user with ID '{request.UserId}'");

        // Map orders to DTOs
        return orders
            .Select(order => OrderDto.FromEntity(order))
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }
}
