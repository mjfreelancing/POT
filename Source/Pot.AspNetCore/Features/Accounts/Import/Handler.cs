using AllOverIt.Logging.Extensions;
using CsvHelper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Services;
using Pot.AspNetCore.ProblemDetails.Extensions;
using Pot.AspNetCore.Validation;
using Pot.AspNetCore.Validation.Extensions;
using System.Globalization;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal sealed class Handler
{
    // Refer to this link for examples and security considerations:
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-9.0

    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([FromForm] Request request,
        IProblemDetailsInspector problemDetailsInspector, IImportAccountService accountImportService,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        using var reader = new StreamReader(request.File.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var accounts = csv.GetRecords<AccountForImport>().ToArray();

        var problemDetails = problemDetailsInspector.Validate(accounts);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var importSummary = await accountImportService.ImportAccountsAsync(accounts, request.Overwrite, cancellationToken);

        return Response.Ok(importSummary);
    }
}
