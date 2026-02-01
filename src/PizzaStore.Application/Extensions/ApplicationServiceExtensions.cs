using Microsoft.Extensions.DependencyInjection;
using PizzaStore.Application.Services;

namespace PizzaStore.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        
        return services;
    }
}
