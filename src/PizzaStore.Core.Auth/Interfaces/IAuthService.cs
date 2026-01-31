using PizzaStore.Core.Auth.DTOs;

namespace PizzaStore.Core.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto);
}
