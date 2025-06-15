using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Projections;

namespace Pot.AspNetCore.Features.Projections.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke(
        IProjectionsService projectionsService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        await projectionsService.GetProjectionsAsync(cancellationToken);

        return Response.Ok();
    }
}
