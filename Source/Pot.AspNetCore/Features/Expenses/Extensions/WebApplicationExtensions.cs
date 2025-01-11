using CsvHelper;
using Pot.AspNetCore.Features.Expenses.Import.Models;
using Pot.AspNetCore.Features.Expenses.Import.Repository;
using Pot.Data.Dtos;
using System.Globalization;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication AddExpenseEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Expense Routes]"))
        {
            app.Logger.LogInformation("Adding expense endpoints");

            var group = app
                .MapGroup("api/expenses")
                .WithTags("Expenses Api");

            group
               .MapGet("", GetAll.Handler.Invoke)
               .Produces<List<ExpenseDto>>();

            group
                .MapPost("/import", async (IFormFile file, IExpenseImportRepository importRepository, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        using var reader = new StreamReader(file.OpenReadStream());
                        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                        var expenses = csv.GetRecords<ExpenseImport>().ToArray();

                        await importRepository.ImportExpensesAsync(expenses, cancellationToken);
                    }
                    catch (Exception)
                    {
                        return Results.BadRequest("Invalid file.");
                    }

                    return Results.Ok();
                })
                .Accepts<IFormFile>("multipart/form-data")
                .DisableAntiforgery();
        }

        return app;
    }
}
