using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Order.CheckoutCart;

/// <summary>
/// Handles checkout process, converting user's cart into an order
/// </summary>
public class CheckoutCartCommandHandler : IRequestHandler<CheckoutCartCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CheckoutCartCommandHandler> _logger;

    public CheckoutCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<CheckoutCartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();
        
        _logger.LogInformation("User {UserId} initiating cart checkout", userId);

        // Get user's cart with items
        var cart = await _unitOfWork.Carts.GetCartWithItemsByUserIdAsync(userId);
        if (cart == null || cart.CartItems.Count == 0)
        {
            _logger.LogWarning("User {UserId} attempted checkout with empty cart", userId);
            throw new ValidationException("Cart is empty. Cannot proceed with checkout");
        }

        // Validate all items still available and calculate total
        decimal orderTotalPrice = 0;
        var orderItems = new List<OrderItem>();
        var orderItemToppings = new List<OrderItemTopping>();

        foreach (var cartItem in cart.CartItems)
        {
            // Validate pizza variant still exists
            if (cartItem.PizzaVariant == null)
                throw new NotFoundException($"Pizza variant for cart item '{cartItem.Id}' no longer exists");

            // Issue #12: Verify variant is still available
            if (!cartItem.PizzaVariant.IsAvailable)
            {
                throw new ValidationException($"Pizza variant '{cartItem.PizzaVariant.Pizza.Name} ({cartItem.PizzaVariant.Size})' is no longer available.");
            }

            // Validate all toppings still exist
            foreach (var cartTopping in cartItem.CartItemToppings)
            {
                if (cartTopping.Topping == null)
                    throw new NotFoundException($"Topping for cart item '{cartItem.Id}' no longer exists");

                // Issue #12: Verify topping is still available
                if (!cartTopping.Topping.IsAvailable)
                {
                    throw new ValidationException($"Topping '{cartTopping.Topping.Name}' is no longer available.");
                }
            }

            // Calculate item price (variant price + toppings)
            var itemPrice = cartItem.CalculateItemPrice();
            var subtotal = cartItem.CalculateSubtotal();

            // Create OrderItem with snapshot data
            var orderItem = new OrderItem
            {
                PizzaVariantId = cartItem.PizzaVariantId,
                PizzaNameAtOrder = cartItem.PizzaVariant.Pizza.Name,
                PizzaSizeAtOrder = cartItem.PizzaVariant.Size.ToString(),
                PizzaBasePriceAtOrder = cartItem.PizzaVariant.Price,
                Quantity = cartItem.Quantity,
                SpecialInstructions = cartItem.SpecialInstructions,
                SubtotalAtOrder = subtotal
            };

            // Create OrderItemToppings with snapshot data
            foreach (var cartTopping in cartItem.CartItemToppings)
            {
                var orderItemTopping = new OrderItemTopping
                {
                    ToppingId = cartTopping.ToppingId,
                    ToppingNameAtOrder = cartTopping.Topping.Name,
                    ToppingPriceAtOrder = cartTopping.Topping.Price
                };
                orderItemToppings.Add(orderItemTopping);
                orderItem.OrderItemToppings.Add(orderItemTopping);
            }

            orderItems.Add(orderItem);
            orderTotalPrice += subtotal;
        }

        // Issue #11: Validate minimum order total
        const decimal MinimumOrderTotal = 5.00m;
        if (orderTotalPrice < MinimumOrderTotal)
        {
            _logger.LogWarning("User {UserId} failed checkout: order total {OrderTotal:C} below minimum {MinimumTotal:C}", 
                userId, orderTotalPrice, MinimumOrderTotal);
            throw new ValidationException($"Minimum order total is ${MinimumOrderTotal:F2}. Your cart total is ${orderTotalPrice:F2}.");
        }

        // Create Order with snapshot data
        var order = new Domain.Entities.Order
        {
            UserId = userId,
            TotalPrice = orderTotalPrice,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        // Begin explicit transaction for checkout operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Add order to context
            await _unitOfWork.Orders.AddAsync(order);
            
            // Clear cart items (mark for deletion)
            foreach (var cartItem in cart.CartItems.ToList()) // ToList to avoid collection modification
            {
                await _unitOfWork.CartItems.DeleteAsync(cartItem);
            }
            
            // Save all changes atomically (order creation + cart deletion)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Commit transaction - all or nothing
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Rollback if anything fails - keeps data consistent
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Checkout failed for user {UserId}: {ErrorMessage}", userId, ex.Message);
            throw;
        }

        _logger.LogInformation("Order {OrderId} created successfully for user {UserId} with total {Total:C}", 
            order.Id, userId, orderTotalPrice);

        // Reload order with details
        var createdOrder = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(order.Id);
        if (createdOrder == null)
            throw new NotFoundException($"Order with ID '{order.Id}' not found after creation");

        return OrderDto.FromEntity(createdOrder);
    }
}
