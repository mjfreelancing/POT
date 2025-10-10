using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pot.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonFromOptions<TType>(this IServiceCollection services)
        where TType : class
    {
        // Allow for injection of TType instead of IOptions<TType>
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<TType>>().Value);

        return services;
    }
}
