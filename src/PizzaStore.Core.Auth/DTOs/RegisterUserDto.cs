namespace PizzaStore.Core.Auth.DTOs;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
