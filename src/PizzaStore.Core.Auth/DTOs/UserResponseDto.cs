namespace PizzaStore.Core.Auth.DTOs;

public record UserResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email
);
