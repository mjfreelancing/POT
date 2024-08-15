using Pot.Maui.Features.Expenses.ViewModels;
using Pot.Maui.Features.Expenses.Views;

namespace Pot.Maui.Features.Expenses.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExpenses(this IServiceCollection services)
    {
        services.AddScoped<ExpensesViewModel>();
        services.AddScoped<ExpensesPage>();

        return services;
    }
}
