using CommunityToolkit.Maui;
using Pot.Maui.Features.Calculators.Allocation;

namespace Pot.Maui.Features.Calculators.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCalculators(this IServiceCollection services)
    {
        services.AddScoped<IAllocationCalculator, StandardAllocationCalculator>();

        return services;
    }
}
