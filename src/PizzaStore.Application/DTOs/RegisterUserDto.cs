namespace PizzaStore.Application.DTOs;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
