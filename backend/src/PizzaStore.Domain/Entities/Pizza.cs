using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class Pizza : BaseEntity
{
    [Required(ErrorMessage = "Pizza name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public PizzaType Type { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public ICollection<PizzaVariant> Variants { get; set; } = new List<PizzaVariant>();
}
