using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Admin.UpdateOrderStatus;

/// <summary>
/// Handles administrative updates to order status with validation
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can update order status");

        _logger.LogInformation("Admin updating order {OrderId} status from current to {NewStatus}", 
            request.OrderId, request.NewStatus);

        // Get the order
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Admin attempted to update non-existent order {OrderId}", request.OrderId);
            throw new NotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        // Validate status transition
        var previousStatus = order.Status;
        ValidateStatusTransition(order.Status, request.NewStatus);

        // Update status and set appropriate timestamps
        order.Status = request.NewStatus;

        // Set timestamps based on new status
        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.CompletedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                break;
        }

        // Save changes - EF Core tracks changes automatically
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Order {OrderId} status updated from {PreviousStatus} to {NewStatus} for user {UserId}", 
            request.OrderId, previousStatus, request.NewStatus, order.UserId);

        // Reload order with details
        var updatedOrder = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(order.Id);
        if (updatedOrder == null)
            throw new NotFoundException($"Order with ID '{order.Id}' not found after update");

        return OrderDto.FromEntity(updatedOrder);
    }

    private static void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Can't transition from Delivered back to any other status except remaining Delivered
        if (currentStatus == OrderStatus.Delivered && newStatus != OrderStatus.Delivered)
            throw new ValidationException($"Cannot transition from Delivered to {newStatus}. Delivered orders cannot be changed");

        // Can't transition from Cancelled back to any other status except remaining Cancelled
        if (currentStatus == OrderStatus.Cancelled && newStatus != OrderStatus.Cancelled)
            throw new ValidationException($"Cannot transition from Cancelled to {newStatus}. Cancelled orders cannot be changed");

        // Additional validation rules could be added here
        // For example: Pending -> Confirmed -> Preparing -> OutForDelivery -> Delivered
    }
}
