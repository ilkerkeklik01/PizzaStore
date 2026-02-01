using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Order.CancelOrder;

/// <summary>
/// Handles cancellation of pending or confirmed orders by the order owner
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<CancelOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();
        
        _logger.LogInformation("User {UserId} attempting to cancel order {OrderId}", userId, request.OrderId);

        // Get order
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            _logger.LogWarning("User {UserId} attempted to cancel non-existent order {OrderId}", userId, request.OrderId);
            throw new NotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        // Verify ownership
        if (order.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel order {OrderId} owned by {OwnerId}", 
                userId, request.OrderId, order.UserId);
            throw new UnauthorizedException("You do not have permission to cancel this order");
        }

        // Validate status is Pending or Confirmed only
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            _logger.LogWarning("User {UserId} attempted to cancel order {OrderId} with invalid status {Status}", 
                userId, request.OrderId, order.Status);
            throw new ValidationException($"Cannot cancel order with status '{order.Status}'. Only Pending or Confirmed orders can be cancelled");
        }

        // Update status to Cancelled
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        // Save changes - EF Core tracks changes automatically
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Order {OrderId} successfully cancelled by user {UserId}", request.OrderId, userId);

        // Reload order with details
        var updatedOrder = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(order.Id);
        if (updatedOrder == null)
            throw new NotFoundException($"Order with ID '{order.Id}' not found after cancellation");

        return OrderDto.FromEntity(updatedOrder);
    }
}
