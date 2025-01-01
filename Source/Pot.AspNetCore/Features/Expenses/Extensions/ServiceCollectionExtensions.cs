using Pot.AspNetCore.Features.Expenses.Import.Repository;

namespace Pot.AspNetCore.Features.Expenses.GetAll.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExpenseServicess(this IServiceCollection services)
    {
        services.AddScoped<IExpenseImportRepository, ExpenseImportRepository>();

        return services;
    }
}
