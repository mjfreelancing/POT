using CommunityToolkit.Maui;
using Pot.Features.Calculators.Allocation;

namespace Pot.Features.Calculators.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCalculators(this IServiceCollection services)
    {
        services.AddScoped<IAllocationCalculator, StandardAllocationCalculator>();

        return services;
    }
}
