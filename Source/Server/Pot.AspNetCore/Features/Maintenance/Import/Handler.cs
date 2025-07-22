using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.App.Features.Maintenance.Import;

namespace Pot.AspNetCore.Features.Maintenance.Import;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([FromForm] Request request,
        IImportDataService importAccountsService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        using var zipStream = request.File.OpenReadStream();

        // TODO: Add validation so the result needs to cater for success and failure.
        // TODO: This currently returns the total number of records imported, extend it to return more detailed information.
        var result = await importAccountsService.ImportAsync(zipStream, cancellationToken);

        return Response.Ok(result);
    }
}