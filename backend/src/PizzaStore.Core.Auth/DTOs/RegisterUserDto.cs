namespace PizzaStore.Core.Auth.DTOs;

/// <summary>
/// Data transfer object for new user registration
/// </summary>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password</param>
public record RegisterUserDto(
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
    string Email,
    
    /// <summary>
    /// User's password
    /// </summary>
    /// <example>SecurePassword123!</example>
    string Password
);
