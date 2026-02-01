using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class CartItem : BaseEntity
{
    public string CartId { get; set; } = string.Empty;
    public Cart Cart { get; set; } = null!;
    
    public string PizzaVariantId { get; set; } = string.Empty;
    public PizzaVariant PizzaVariant { get; set; } = null!;
    
    [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
    public int Quantity { get; set; }

    [MaxLength(500)]
    public string? SpecialInstructions { get; set; } = string.Empty;
    
    public ICollection<CartItemTopping> CartItemToppings { get; set; } = new List<CartItemTopping>();
}
