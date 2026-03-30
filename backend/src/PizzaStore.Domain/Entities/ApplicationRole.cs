using Microsoft.AspNetCore.Identity;

namespace PizzaStore.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public DateTime CreatedAt { get; set; }
}
