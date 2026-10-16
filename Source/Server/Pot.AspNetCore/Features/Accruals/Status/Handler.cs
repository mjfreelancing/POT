using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Concerns.Time;
using Pot.App.Features.Accruals.Status;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Accruals.Status;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request, IAccrualsStatusService accrualsStatusService,
        ITimeProvider timeProvider, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput(timeProvider.GetLocalDateNow());

        var statusResult = await accrualsStatusService.GetStatusAsync(input, cancellationToken);

        return statusResult.IsSuccess
            ? Response.Ok(statusResult.Value!)
            : TypedResults.Problem(statusResult.Error!.ToProblemDetails());
    }
}
