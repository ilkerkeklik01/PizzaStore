namespace PizzaStore.Core.Auth.DTOs;

/// <summary>
/// Response containing authentication token and user information
/// </summary>
/// <param name="Token">JWT authentication token</param>
/// <param name="User">User information</param>
public record AuthResponseDto(
    /// <summary>
    /// JWT authentication token for API access
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string Token,
    
    /// <summary>
    /// User information for the authenticated user
    /// </summary>
    UserResponseDto User
);
