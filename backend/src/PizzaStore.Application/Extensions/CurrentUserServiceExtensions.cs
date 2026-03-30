using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;

namespace PizzaStore.Application.Extensions;

public static class CurrentUserServiceExtensions
{
    public static string GetAuthenticatedUserId(this ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("User is not authenticated");
        return userId;
    }
}
