using AllOverIt.Logging.Extensions;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Services;
using System.Globalization;
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal sealed class Handler
{
    // Refer to this link for examples and security considerations:
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0

    internal const int ExpectedColumnCount = 6;
    internal static readonly ProblemDetails _invalidCsvColumnCount = ApiProblemDetailsFactory.CreateUnprocessableEntity($"Invalid CSV file format. Expected {ExpectedColumnCount} header columns.");

    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([FromForm] Request request,
        IImportAccountService accountImportService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        try
        {
            using var reader = new StreamReader(request.File.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            var columnCount = csv.ColumnCount;

            if (columnCount != ExpectedColumnCount)
            {
                return TypedResults.Problem(_invalidCsvColumnCount);
            }

            var importSummary = await accountImportService.ImportAccountsAsync(csv.GetRecords<AccountCsvRow>(), cancellationToken);

            return importSummary.IsSuccess
                ? Response.Ok(importSummary.Value!)
                : TypedResults.Problem(importSummary.Error!.GetProblemDetails());
        }
        catch (TypeConverterException exception)
        {
            var value = exception.Text;
            var reader = exception.Context?.Reader;
            var currentIndex = reader?.CurrentIndex;

            var headerName = currentIndex is not null
                ? reader!.HeaderRecord?[currentIndex.Value]
                : null;

            var detail = headerName is not null
                ? $"Unable to convert '{value}' for header '{headerName}'"
                : exception.Message;

            return TypedResults.Problem(
                statusCode: (int)HttpStatusCode.UnprocessableEntity,
                detail: detail);
        }
    }
}
