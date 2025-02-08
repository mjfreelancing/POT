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
                .MapGroup("api/expenses")
                .WithTags("Expenses Api")
                .GetAllExpenses()
                //.GetExpense()
                //.CreateExpense()
                //.UpdateExpense()
                //.DeleteExpense()
                .ImportExpenses(MaxImportPayloadBytes);
        }

        return app;
    }
}
