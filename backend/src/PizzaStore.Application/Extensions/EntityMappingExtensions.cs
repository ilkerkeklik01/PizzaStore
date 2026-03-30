using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Extensions;

public static class EntityMappingExtensions
{
    public static decimal CalculateItemPrice(this CartItem cartItem)
    {
        var basePrice = cartItem.PizzaVariant?.Price ?? 0;
        var toppingsPrice = cartItem.CartItemToppings?.Sum(t => t.Topping?.Price ?? 0) ?? 0;
        return basePrice + toppingsPrice;
    }
    
    public static decimal CalculateSubtotal(this CartItem cartItem)
    {
        return cartItem.CalculateItemPrice() * cartItem.Quantity;
    }
}
