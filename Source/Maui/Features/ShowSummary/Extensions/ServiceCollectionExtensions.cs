using CommunityToolkit.Maui;
using Pot.Maui.Features.ShowSummary.Services;
using Pot.Maui.Features.ShowSummary.ViewModels;
using Pot.Maui.Features.ShowSummary.Views;

namespace Pot.Maui.Features.ShowSummary.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSummary(this IServiceCollection services)
    {
        services.AddTransient<ISummaryViewModel, SummaryViewModel>();
        services.AddTransient<SummaryPage>();

        services.AddTransient<ISummaryService, SummaryService>();

        return services;
    }
}
