using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Order.GetMyOrders;

/// <summary>
/// Retrieves all orders for the authenticated user
/// </summary>
public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Get all orders for current user, ordered by CreatedAt descending
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

        // Map to DTOs
        var orderDtos = orders
            .Select(order => OrderDto.FromEntity(order))
            .ToList();

        return orderDtos;
    }
}
