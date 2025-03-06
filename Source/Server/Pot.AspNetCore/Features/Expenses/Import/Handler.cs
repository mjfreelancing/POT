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
using System.Net;

namespace Pot.AspNetCore.Features.Expenses.Import;

internal sealed class Handler
{
    // Refer to this link for examples and security considerations:
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0

    internal static readonly ProblemDetails _invalidCsvColumnCount = ApiProblemDetailsFactory.CreateUnprocessableEntity("Invalid CSV file format. Expected 10 header columns.");

    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([FromForm] Request request,
        IImportExpenseService expenseImportService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        try
        {
            using var reader = new StreamReader(request.File.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            var columnCount = csv.ColumnCount;

            if (columnCount != 10)
            {
                return TypedResults.Problem(_invalidCsvColumnCount);
            }

            var importSummary = await expenseImportService.ImportExpensesAsync(csv.GetRecords<ExpenseCsvRow>(), cancellationToken);

            return importSummary.IsSuccess
                ? Response.Ok(importSummary.Value!)
                : TypedResults.Problem(importSummary.Error!.GetProblemDetails());
        }
        catch (TypeConverterException exception)
        {
            // TODO: Test / Handle various things that could go wrong with an incorrectly formatted CSV file
            //   Invalid CSV file format - too few / many columns
            //   Column conversions, such as AccountId to a Guid, DateOnly values etc
            //
            //var value = exception.Text;
            //var columnCount = exception.Context.Reader.ColumnCount;
            //var currentIndex = exception.Context.Reader.CurrentIndex;
            //var header = exception.Context.Reader.HeaderRecord[currentIndex];

            return TypedResults.Problem(statusCode: (int)HttpStatusCode.UnprocessableEntity, detail: exception.Message);
        }
    }
}
