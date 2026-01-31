namespace PizzaStore.Core.Auth.DTOs;

public record LoginUserDto(
    string Email,
    string Password
);
