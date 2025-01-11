using Pot.AspNetCore.Features.Expenses.Import.Repository;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddExpenseServicess(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IExpenseImportRepository, ExpenseImportRepository>();

        return builder;
    }
}
