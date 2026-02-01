using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;
using PizzaStore.Application.Features.Commands.Cart.IncreaseCartItemQuantity;
using PizzaStore.Application.Features.Commands.Cart.DecreaseCartItemQuantity;
using PizzaStore.Application.Features.Commands.Cart.RemoveCartItem;
using PizzaStore.Application.Features.Commands.Cart.ClearCart;
using PizzaStore.Application.Features.Queries.Cart.GetUserCart;
using PizzaStore.Application.Features.Queries.Cart.GetCartItem;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    /// <returns>User's cart with all items, toppings, and total price</returns>
    /// <response code="200">Returns the user's cart</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserCart()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
            
        var query = new GetUserCartQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Add pizza to cart
    /// </summary>
    /// <param name="dto">Cart item data including pizza variant ID, quantity, special instructions, and topping IDs</param>
    /// <returns>Created cart item details</returns>
    /// <response code="201">Item added to cart successfully</response>
    /// <response code="400">If the cart item data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddPizzaToCart([FromBody] AddPizzaToCartDto dto)
    {
        var command = new AddPizzaToCartCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCartItem), new { cartItemId = result.Id }, result);
    }

    /// <summary>
    /// Get specific cart item
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item</param>
    /// <returns>Cart item details with pizza, toppings, and pricing</returns>
    /// <response code="200">Returns the cart item details</response>
    /// <response code="404">If the cart item is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCartItem(string cartItemId)
    {
        var query = new GetCartItemQuery(cartItemId);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item</param>
    /// <param name="dto">Updated quantity</param>
    /// <returns>Updated cart item details</returns>
    /// <response code="200">Quantity updated successfully</response>
    /// <response code="400">If the quantity is invalid</response>
    /// <response code="404">If the cart item is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPut("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCartItemQuantity(string cartItemId, [FromBody] UpdateCartItemQuantityDto dto)
    {
        var command = new UpdateCartItemQuantityCommand(dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Increase cart item quantity by 1
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item</param>
    /// <returns>Updated cart item details</returns>
    /// <response code="200">Quantity increased successfully</response>
    /// <response code="404">If the cart item is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPatch("items/{cartItemId}/increase")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IncreaseCartItemQuantity(string cartItemId)
    {
        var command = new IncreaseCartItemQuantityCommand(cartItemId, 1);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Decrease cart item quantity by 1
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item</param>
    /// <returns>Updated cart item details (if quantity > 1) or deletion confirmation (if quantity becomes 0)</returns>
    /// <response code="200">Quantity decreased successfully</response>
    /// <response code="404">If the cart item is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPatch("items/{cartItemId}/decrease")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DecreaseCartItemQuantity(string cartItemId)
    {
        var command = new DecreaseCartItemQuantityCommand(cartItemId, 1);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item to remove</param>
    /// <returns>No content on successful removal</returns>
    /// <response code="204">Item removed from cart successfully</response>
    /// <response code="404">If the cart item is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpDelete("items/{cartItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveCartItem(string cartItemId)
    {
        var command = new RemoveCartItemCommand(cartItemId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    /// <returns>No content on successful clearance</returns>
    /// <response code="204">Cart cleared successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart()
    {
        var command = new ClearCartCommand();
        await _mediator.Send(command);
        return NoContent();
    }
}
