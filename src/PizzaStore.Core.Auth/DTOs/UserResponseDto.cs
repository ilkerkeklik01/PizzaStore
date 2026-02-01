namespace PizzaStore.Core.Auth.DTOs;

/// <summary>
/// Represents basic user information returned after authentication
/// </summary>
/// <param name="Id">Unique identifier for the user</param>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
/// <param name="Email">User's email address</param>
public record UserResponseDto(
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    /// <example>auth0|123456789</example>
    string Id,
    
    /// <summary>
    /// User's first name
    /// </summary>
    /// <example>John</example>
    string FirstName,
    
    /// <summary>
    /// User's last name
    /// </summary>
    /// <example>Doe</example>
    string LastName,
    
    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    string Email
);
