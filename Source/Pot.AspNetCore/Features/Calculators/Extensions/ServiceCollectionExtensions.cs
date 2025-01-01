using Pot.AspNetCore.Features.Calculators.Allocation;

namespace Pot.AspNetCore.Features.Calculators.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCalculators(this IServiceCollection services)
    {
        services.AddScoped<IAllocationCalculator, StandardAllocationCalculator>();

        return services;
    }
}
