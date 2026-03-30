using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PizzaStore.Application.Common.Behaviors;
using PizzaStore.Application.Features.Auth.Commands.Register;
using PizzaStore.Application.Services;

namespace PizzaStore.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
        });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
