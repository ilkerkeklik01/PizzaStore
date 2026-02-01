using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class PizzaVariant : BaseEntity
{
    public string PizzaId { get; set; } = string.Empty;
    public Pizza Pizza { get; set; } = null!;
    
    public PizzaSize Size { get; set; }

    [Required]
    [Range(0.01, 9999.99, ErrorMessage = "Price must be between $0.01 and $9999.99")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}
