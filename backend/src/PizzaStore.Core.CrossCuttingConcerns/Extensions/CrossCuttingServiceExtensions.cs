using Microsoft.AspNetCore.Builder;
using PizzaStore.Core.CrossCuttingConcerns.Middleware;

namespace PizzaStore.Core.CrossCuttingConcerns.Extensions;

public static class CrossCuttingServiceExtensions
{
    public static IApplicationBuilder UseCrossCuttingConcerns(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        return app;
    }
}
