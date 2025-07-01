namespace Pot.AspNetCore.Features.Expenses.Extensions;

internal static class WebApplicationExtensions
{
    private const long MaxImportPayloadBytes = 1 * 1024 * 1024;

    public static WebApplication AddExpenseEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Expense Routes]"))
        {
            app.Logger.LogInformation("Adding expense endpoints");

            var group = app
                .MapGroup(ExpensesEndpoints.Group)
                .WithTags(ExpensesEndpoints.Tag)
                .GetAllExpenses()
                .GetExpense()
                .CreateExpense()
                .UpdateExpense()
                .DeleteExpense()
                .RenewExpenses()
                .ImportExpenses(MaxImportPayloadBytes);
        }

        return app;
    }
}
