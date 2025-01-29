using Pot.Data.Dtos;
using System.Net;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

internal static class WebApplicationExtensions
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
                .MapPost("/import", Import.Handler.Invoke)
                //.WithName(nameof(ImportExpenses))
                .WithSummary("Import Expenses")
                .WithDescription("Import new / update existing expense details")
                .WithTags("Accounts", "Import")
                //.WithMetadata(new RequestSizeLimitAttribute(maxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
                .DisableAntiforgery()
                .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

            //.MapPost("/import", async (IFormFile file, IExpenseImportRepository importRepository, CancellationToken cancellationToken) =>
            //{
            //    try
            //    {
            //        using var reader = new StreamReader(file.OpenReadStream());
            //        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            //        var expenses = csv.GetRecords<ExpenseForImport>().ToArray();

            //        await importRepository.ImportExpensesAsync(expenses, cancellationToken);
            //    }
            //    catch (Exception)
            //    {
            //        return Results.BadRequest("Invalid file.");
            //    }

            //    return Results.Ok();
            //})
            //.Accepts<IFormFile>("multipart/form-data")
            //.DisableAntiforgery();
        }

        return app;
    }
}
