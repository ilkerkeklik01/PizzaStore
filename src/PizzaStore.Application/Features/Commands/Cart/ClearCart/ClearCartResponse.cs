namespace PizzaStore.Application.Features.Commands.Cart.ClearCart;

public class ClearCartResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ItemsRemoved { get; set; }
}
