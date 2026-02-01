namespace PizzaStore.Application.Features.Queries.Admin;

/// <summary>
/// Represents user information for administrative purposes
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    /// <example>auth0|123456789</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Username of the user
    /// </summary>
    /// <example>john.doe</example>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the user
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number of the user
    /// </summary>
    /// <example>+1-555-123-4567</example>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// List of roles assigned to the user
    /// </summary>
    /// <example>["Customer", "Admin"]</example>
    public List<string> Roles { get; set; } = new();

    public static UserDto FromEntity(Domain.Entities.ApplicationUser user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Roles = roles.ToList()
        };
    }
}
