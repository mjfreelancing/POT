using AllOverIt.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.DependencyInjection;
using Pot.Data.Repositories;
using Pot.Data.UnitOfWork;

namespace Pot.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDataDependencies(this IServiceCollection services)
    {
        services.AutoRegisterScoped<PotDataRegistrar>([typeof(IGenericRepository<,>)], config =>
        {
            // Exclude interfaces we know we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                return !serviceType.IsGenericType || serviceType.GetGenericTypeDefinition() != typeof(IGenericRepository<,>);
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
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        // When injecting into a handler / service that performs all operations on the same thread.
        services.AddScoped<IPotUnitOfWork, PotUnitOfWork>();

        // Inject this when multiple units of work need to be created.
        // services.AddSingleton<IPotUnitOfWorkFactory, PotUnitOfWorkFactory>();

        return services;
    }
}