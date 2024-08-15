using CommunityToolkit.Maui;
using Pot.Maui.Features.Summary.Services;
using Pot.Maui.Features.Summary.ViewModels;
using Pot.Maui.Features.Summary.Views;

namespace Pot.Maui.Features.Summary.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSummary(this IServiceCollection services)
    {
        //services.AddTransient<ISummaryViewModel, SummaryViewModel>();
        services.AddTransient<SummaryViewModel>();
        services.AddTransient<SummaryPage>();

        services.AddTransient<ISummaryService, SummaryService>();

        return services;
    }
}
