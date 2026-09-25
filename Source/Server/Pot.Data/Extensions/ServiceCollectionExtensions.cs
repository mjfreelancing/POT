using AllOverIt.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pot.Shared.DependencyInjection;

namespace Pot.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataDependencies(this IServiceCollection services)
    {
        services.AutoRegisterScoped<PotDataRegistrar, IPotScopedDependency>(config =>
        {
            // Exclude interfaces we know we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                return serviceType != typeof(IPotScopedDependency);
            });
        });

        services.AutoRegisterSingleton<PotDataRegistrar, IPotSingletonDependency>(config =>
        {
            // Exclude interfaces we know we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                return serviceType != typeof(IPotSingletonDependency);
            });
        });

        return services;
    }
}