using Microsoft.Extensions.DependencyInjection;

namespace Pot.RazorComponents.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRazorComponentRendering(this IServiceCollection services)
    {
        services.AddScoped<IRazorComponentRenderer>(provider =>
        {
            return new RazorComponentRenderer(provider);
        });

        return services;
    }
}