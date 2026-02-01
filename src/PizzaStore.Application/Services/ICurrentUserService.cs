namespace PizzaStore.Application.Services;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
}
