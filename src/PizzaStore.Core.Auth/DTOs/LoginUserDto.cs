namespace PizzaStore.Core.Auth.DTOs;

/// <summary>
/// Data transfer object for user login credentials
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password</param>
public record LoginUserDto(
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
