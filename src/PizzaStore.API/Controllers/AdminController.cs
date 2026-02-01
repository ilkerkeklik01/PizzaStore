using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Features.Commands.Admin.UpdateOrderStatus;
using PizzaStore.Application.Features.Queries.Admin;
using PizzaStore.Application.Features.Queries.Admin.GetAllUsers;
using PizzaStore.Application.Features.Queries.Admin.GetUserById;
using PizzaStore.Application.Features.Queries.Admin.GetOrdersByUserId;
using PizzaStore.Application.Features.Queries.Admin.GetAllOrders;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Domain.Entities;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    /// <returns>List of all registered users with their information</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <returns>User details including email, name, and roles</returns>
    /// <response code="200">Returns the user details</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Get orders by user ID (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <returns>List of all orders for the specified user</returns>
    /// <response code="200">Returns the list of user's orders</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpGet("users/{id}/orders")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrdersByUserId(string id)
    {
        var query = new GetOrdersByUserIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all orders with optional filters (Admin only)
    /// </summary>
    /// <param name="status">Filter by order status (Pending, Confirmed, Preparing, OutForDelivery, Delivered, Cancelled)</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="fromDate">Filter orders from this date (inclusive)</param>
    /// <param name="toDate">Filter orders up to this date (inclusive)</param>
    /// <returns>List of orders matching the filter criteria</returns>
    /// <response code="200">Returns the filtered list of orders</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetAllOrdersQuery(status, userId, fromDate, toDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <param name="newStatus">New order status (0=Pending, 1=Confirmed, 2=Preparing, 3=OutForDelivery, 4=Delivered, 5=Cancelled)</param>
    /// <returns>Updated order with new status</returns>
    /// <response code="200">Order status updated successfully</response>
    /// <response code="400">If the status transition is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPut("orders/{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] OrderStatus newStatus)
    {
        var command = new UpdateOrderStatusCommand(id, newStatus);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
