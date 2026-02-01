using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Features.Commands.Topping.CreateTopping;
using PizzaStore.Application.Features.Commands.Topping.UpdateTopping;
using PizzaStore.Application.Features.Commands.Topping.DeleteTopping;
using PizzaStore.Application.Features.Queries.Topping;
using PizzaStore.Application.Features.Queries.Topping.DTOs;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToppingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ToppingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all toppings
    /// </summary>
    /// <returns>List of all available toppings with prices</returns>
    /// <response code="200">Returns the list of toppings</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ToppingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllToppings()
    {
        var query = new GetAllToppingsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get topping by ID
    /// </summary>
    /// <param name="id">The unique identifier of the topping</param>
    /// <returns>Topping details with name and price</returns>
    /// <response code="200">Returns the topping details</response>
    /// <response code="404">If the topping is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ToppingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetToppingById(string id)
    {
        var query = new GetAllToppingsQuery();
        var result = await _mediator.Send(query);
        var topping = result.FirstOrDefault(t => t.Id == id);
        
        if (topping == null)
            return NotFound();
        
        return Ok(topping);
    }

    /// <summary>
    /// Create a new topping (Admin only)
    /// </summary>
    /// <param name="dto">Topping data including name and price</param>
    /// <returns>Created topping details</returns>
    /// <response code="201">Topping created successfully</response>
    /// <response code="400">If the topping data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateToppingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTopping([FromBody] CreateToppingDto dto)
    {
        var command = new CreateToppingCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetToppingById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update topping (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the topping to update</param>
    /// <param name="dto">Updated topping data</param>
    /// <returns>Updated topping details</returns>
    /// <response code="200">Topping updated successfully</response>
    /// <response code="400">If the update data is invalid</response>
    /// <response code="404">If the topping is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UpdateToppingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTopping(string id, [FromBody] UpdateToppingDto dto)
    {
        var command = new UpdateToppingCommand(id, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete topping (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the topping to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Topping deleted successfully</response>
    /// <response code="404">If the topping is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTopping(string id)
    {
        var command = new DeleteToppingCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}
