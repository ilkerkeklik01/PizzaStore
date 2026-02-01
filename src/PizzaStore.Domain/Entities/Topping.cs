using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class Topping : BaseEntity
{
    [Required(ErrorMessage = "Topping name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 99.99, ErrorMessage = "Price must be between $0.01 and $99.99")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}
