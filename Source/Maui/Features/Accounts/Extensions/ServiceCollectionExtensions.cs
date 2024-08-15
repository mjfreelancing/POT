using Pot.Maui.Features.Accounts.ViewModels;
using Pot.Maui.Features.Accounts.Views;

namespace Pot.Maui.Features.Accounts.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddScoped<AccountsViewModel>();
        services.AddScoped<AccountsPage>();

        return services;
    }
}
