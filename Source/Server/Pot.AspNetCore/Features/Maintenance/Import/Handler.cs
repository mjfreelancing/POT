using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.App.Features.Maintenance.Import;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Maintenance.Import;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([FromForm] Request request,
        IImportDataService importAccountsService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        using var zipStream = request.File.OpenReadStream();

        var result = await importAccountsService.ImportAsync(zipStream, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.GetProblemDetails());
    }
}