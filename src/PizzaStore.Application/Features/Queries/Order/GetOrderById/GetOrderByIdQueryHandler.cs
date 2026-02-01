using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Order.GetOrderById;

/// <summary>
/// Retrieves a specific order by ID with authorization check
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Get order by id with full details
        var order = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(request.OrderId);
        if (order == null)
            throw new NotFoundException($"Order with ID '{request.OrderId}' not found");

        // Verify ownership
        if (order.UserId != userId)
            throw new UnauthorizedException("You do not have permission to view this order");

        return OrderDto.FromEntity(order);
    }
}
