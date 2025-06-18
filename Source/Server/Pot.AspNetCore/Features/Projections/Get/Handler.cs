using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Projections;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Projections.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke(
        IProjectionsService projectionsService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var options = new ProjectionOptions
        {
            DaysForecast = 365
        };

        var output = await projectionsService.GetProjectionsAsync(options, cancellationToken);

        return output.IsSuccess
            ? Response.Ok(output.Value!)
            : TypedResults.Problem(output.Error!.GetProblemDetails());
    }
}
