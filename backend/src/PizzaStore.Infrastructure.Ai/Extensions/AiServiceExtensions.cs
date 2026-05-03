using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace PizzaStore.Infrastructure.Ai.Extensions;

public static class AiServiceExtensions
{
    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not configured.");

        var apiKey = configuration["AZURE_OPENAI_API_KEY"]
            ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not configured.");

        var deployment = configuration["AZURE_OPENAI_DEPLOYMENT"]
            ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT is not configured.");

        // Kernel is registered as transient — it is a lightweight container and safe to recreate per request
        services.AddTransient<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deployment,
                endpoint: endpoint,
                apiKey: apiKey);

            return builder.Build();
        });

        return services;
    }
}
