using AllOverIt.Logging.Extensions;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Expenses.Import.Models;
using Pot.AspNetCore.Features.Expenses.Import.Services;
using System.Globalization;

namespace Pot.AspNetCore.Features.Expenses.Import;

internal sealed class Handler
{
    // Refer to this link for examples and security considerations:
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0

    internal static readonly ProblemDetails _invalidCsvColumnCount = ApiProblemDetailsFactory.CreateUnprocessableEntity("Invalid CSV file format. Expected 9 header columns.");

    public static async Task<Results<Ok<Response>, /*StatusCodeHttpResult,*/ ProblemHttpResult>> Invoke([FromForm] Request request,
        IImportExpenseService expenseImportService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        // TODO: Error handling
        // ====================
        // Request
        //   Account Id is a valid GUID - see Pot.AspNetCore.Features.Accounts.Update.RequestValidator => make something re-usable
        //
        // During import
        //   Invalid CSV file format - too few / many columns
        //   Column conversions, such as AccountId to a Guid, DateOnly values etc

        try
        {
            using var reader = new StreamReader(request.File.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            var columnCount = csv.ColumnCount;

            if (columnCount != 9)
            {
                return TypedResults.Problem(_invalidCsvColumnCount);
            }

            IEnumerable<ExpenseForImport> GetExpenses()
            {
                while (csv.Read())
                {
                    yield return csv.GetRecord<ExpenseForImport>();
                }
            }

            var importSummary = await expenseImportService.ImportExpensesAsync(GetExpenses(), request.Overwrite, cancellationToken);

            return importSummary.IsSuccess
                ? Response.Ok(importSummary.Value!)
                : TypedResults.Problem(importSummary.Error!.GetProblemDetails());
        }
        catch (TypeConverterException exception)
        {
            // TODO: Test / Handle various things that could go wrong with an incrrectly formatted CSV file
            var value = exception.Text;
            var columnCount = exception.Context.Reader.ColumnCount;
            var currentIndex = exception.Context.Reader.CurrentIndex;
            var header = exception.Context.Reader.HeaderRecord[currentIndex];

            return TypedResults.Problem(null);
        }
    }
}
