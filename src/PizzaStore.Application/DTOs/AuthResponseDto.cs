namespace PizzaStore.Application.DTOs;

public record AuthResponseDto(
    string Token,
    UserResponseDto User
);
