using CommunityToolkit.Maui;
using Pot.Maui.Features.Login.ViewModels;
using Pot.Maui.Features.Login.Views;

namespace Pot.Maui.Features.Login.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogin(this IServiceCollection services)
    {
        services.AddScoped<ILoginViewModel, LoginViewModel>();
        services.AddScoped<LoginPage>();

        return services;
    }
}
