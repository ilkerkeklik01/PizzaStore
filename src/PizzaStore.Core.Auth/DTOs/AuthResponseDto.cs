namespace PizzaStore.Core.Auth.DTOs;

public record AuthResponseDto(
    string Token,
    UserResponseDto User
);
