using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddExpenseEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Expense Routes]"))
        {
            app.Logger.LogInformation("Adding expense endpoints");

            var group = app
                .MapGroup(ExpensesEndpoints.Group)
                .WithTags(ExpensesEndpoints.Tag)
                .ProducesStandardProblems()
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetAllExpenses()
                .GetExpense()
                .CreateExpense()
                .UpdateExpense()
                .DeleteExpense()
                .RenewExpenses()
                .ToggleExcludeExpenses();
        }

        return app;
    }
}
