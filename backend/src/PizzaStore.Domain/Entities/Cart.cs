using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class Cart : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
