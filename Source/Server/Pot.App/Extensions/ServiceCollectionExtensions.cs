using AllOverIt.DependencyInjection.Extensions;
using AllOverIt.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.DependencyInjection;
using Pot.Data.Extensions;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAppDependencies(this IServiceCollection services)
    {
        services.AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>(config =>
        {
            // Exclude interfaces we know we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                if (serviceType.IsGenericType)
                {
                    var genericTypeDefinition = serviceType.GetGenericTypeDefinition();

                    // Not expecting other types, but only filter out those we expect
                    return !(genericTypeDefinition == typeof(IValidator<>) || genericTypeDefinition == typeof(ValidatorBase<>));
                }

                return serviceType != typeof(IPotScopedDependency);
            });
        });

        //services.AutoRegisterScoped<DependencyRegistrar, IPotSingletonDependency>();
        services.AddDataDependencies();
    }
}
