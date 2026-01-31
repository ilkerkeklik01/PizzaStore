namespace PizzaStore.Application.DTOs;

public record UserResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email
);
