using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Settings.GetAll;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Settings.GetAll;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(IGetAllSettingsService settingsService,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var result = await settingsService.GetAllSettingsAsync(cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}
