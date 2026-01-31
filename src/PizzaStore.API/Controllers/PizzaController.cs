using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PizzaStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    /// <summary>
    /// Get all pizzas - requires authentication
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetAll()
    {
        // Example data - in real app, this would come from database
        var pizzas = new[]
        {
            new { Id = 1, Name = "Margherita", Price = 10.99 },
            new { Id = 2, Name = "Pepperoni", Price = 12.99 },
            new { Id = 3, Name = "Hawaiian", Price = 11.99 }
        };

        return Ok(pizzas);
    }

    /// <summary>
    /// Admin endpoint - only admins can access
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdminData()
    {
        return Ok(new
        {
            Message = "This is admin-only data",
            Statistics = new
            {
                TotalOrders = 150,
                TotalRevenue = 3450.50,
                ActiveUsers = 42
            }
        });
    }
}
