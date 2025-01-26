using Microsoft.Extensions.DependencyInjection;
using Pot.Data.UnitOfWork;

namespace Pot.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        // When injecting into a handler / service that performs all operations on the same thread.
        services.AddScoped<IPotUnitOfWork, PotUnitOfWork>();

        // Inject this when multiple units of work need to be created.
        // services.AddSingleton<IPotUnitOfWorkFactory, PotUnitOfWorkFactory>();

        return services;
    }
}