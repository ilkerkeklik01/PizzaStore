using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Application.Features.Commands.Pizza.CreatePizza;
using PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;
using PizzaStore.Application.Features.Commands.Pizza.DeletePizza;
using PizzaStore.Application.Features.Commands.PizzaVariant.AddPizzaVariant;
using PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;
using PizzaStore.Application.Features.Commands.PizzaVariant.DeletePizzaVariant;
using PizzaStore.Application.Features.Queries.Pizza;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;
using PizzaStore.Domain.Entities;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PizzaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all pizzas
    /// </summary>
    /// <returns>List of all available pizzas with their variants</returns>
    /// <response code="200">Returns the list of pizzas</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PizzaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPizzas()
    {
        var query = new GetAllPizzasQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get pizza by ID
    /// </summary>
    /// <param name="id">The unique identifier of the pizza</param>
    /// <returns>Pizza details with variants</returns>
    /// <response code="200">Returns the pizza details</response>
    /// <response code="404">If the pizza is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PizzaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPizzaById(string id)
    {
        var query = new GetPizzaByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Get pizzas by type
    /// </summary>
    /// <param name="type">Pizza type (Vegetarian, MeatLovers, Hawaiian, Veggie, Custom, Supreme, Margherita)</param>
    /// <returns>List of pizzas matching the specified type</returns>
    /// <response code="200">Returns the list of pizzas of the specified type</response>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(List<PizzaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPizzasByType(PizzaType type)
    {
        var query = new GetPizzasByTypeQuery { Type = type };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new pizza with variants (Admin only)
    /// </summary>
    /// <param name="dto">Pizza creation data including name, description, type, imageUrl, and variants</param>
    /// <returns>Created pizza with all variants</returns>
    /// <response code="201">Pizza created successfully</response>
    /// <response code="400">If the pizza data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreatePizzaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePizza([FromBody] CreatePizzaDto dto)
    {
        var command = new CreatePizzaCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPizzaById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update pizza (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the pizza to update</param>
    /// <param name="dto">Updated pizza data</param>
    /// <returns>Updated pizza details</returns>
    /// <response code="200">Pizza updated successfully</response>
    /// <response code="400">If the update data is invalid</response>
    /// <response code="404">If the pizza is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UpdatePizzaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePizza(string id, [FromBody] UpdatePizzaDto dto)
    {
        var command = new UpdatePizzaCommand(id, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete pizza (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the pizza to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Pizza deleted successfully</response>
    /// <response code="404">If the pizza is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePizza(string id)
    {
        var command = new DeletePizzaCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Add variant to pizza (Admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the pizza</param>
    /// <param name="dto">Variant data including size and price</param>
    /// <returns>Created variant details</returns>
    /// <response code="201">Variant added successfully</response>
    /// <response code="400">If the variant data is invalid</response>
    /// <response code="404">If the pizza is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPost("{id}/variants")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AddPizzaVariantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPizzaVariant(string id, [FromBody] AddPizzaVariantDto dto)
    {
        dto.PizzaId = id; // Set from route parameter
        var command = new AddPizzaVariantCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPizzaById), new { id }, result);
    }

    /// <summary>
    /// Update pizza variant (Admin only)
    /// </summary>
    /// <param name="pizzaId">The unique identifier of the pizza</param>
    /// <param name="variantId">The unique identifier of the variant to update</param>
    /// <param name="dto">Updated variant data</param>
    /// <returns>Updated variant details</returns>
    /// <response code="200">Variant updated successfully</response>
    /// <response code="400">If the update data is invalid</response>
    /// <response code="404">If the pizza or variant is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpPut("{pizzaId}/variants/{variantId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UpdatePizzaVariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePizzaVariant(string pizzaId, string variantId, [FromBody] UpdatePizzaVariantDto dto)
    {
        var command = new UpdatePizzaVariantCommand(variantId, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete pizza variant (Admin only)
    /// </summary>
    /// <param name="pizzaId">The unique identifier of the pizza</param>
    /// <param name="variantId">The unique identifier of the variant to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Variant deleted successfully</response>
    /// <response code="404">If the pizza or variant is not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpDelete("{pizzaId}/variants/{variantId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePizzaVariant(string pizzaId, string variantId)
    {
        var command = new DeletePizzaVariantCommand(variantId);
        await _mediator.Send(command);
        return NoContent();
    }
}
