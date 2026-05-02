using MediatR;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Common.Models;
using PizzaStore.Application.Features.Order.Commands.CheckoutCart;
using PizzaStore.Application.Features.Order.Commands.CancelOrder;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Application.Features.Order.Queries.GetMyOrders;
using PizzaStore.Application.Features.Order.Queries.GetOrderById;
using PizzaStore.Domain.Entities;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Checkout cart and create order
    /// </summary>
    /// <returns>Created order with all items and total amount</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">If the cart is empty or checkout fails</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckoutCart()
    {
        var command = new CheckoutCartCommand();
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get current user's orders with optional filtering and pagination
    /// </summary>
    /// <param name="status">Filter by order status (Pending, Confirmed, Preparing, OutForDelivery, Delivered, Cancelled)</param>
    /// <param name="fromDate">Filter orders from this date (inclusive)</param>
    /// <param name="toDate">Filter orders up to this date (inclusive)</param>
    /// <param name="page">Page number (1-based, default 1)</param>
    /// <param name="pageSize">Number of orders per page (default 10)</param>
    /// <returns>Paginated list of user's orders with items and status</returns>
    /// <response code="200">Returns the paginated list of user's orders</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = new GetMyOrdersQuery(status, fromDate, toDate, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <returns>Order details with all items and status</returns>
    /// <response code="200">Returns the order details</response>
    /// <response code="404">If the order is not found or doesn't belong to the user</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderById(string id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="id">The unique identifier of the order to cancel</param>
    /// <returns>Updated order with Cancelled status</returns>
    /// <response code="200">Order cancelled successfully</response>
    /// <response code="400">If the order cannot be cancelled (already delivered, etc.)</response>
    /// <response code="404">If the order is not found or doesn't belong to the user</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelOrder(string id)
    {
        var command = new CancelOrderCommand(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
