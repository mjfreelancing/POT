using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Concerns.Time;
using Pot.App.Features.Accruals.Status;
using Pot.App.Features.Accruals.Status.Models;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accruals.Status.Mappings;

namespace Pot.AspNetCore.Features.Accruals.Status;

internal sealed class Handler
{
    public static async Task<Results<Ok<Output>, ProblemHttpResult>> Invoke(Request request, IAccrualsStatusService accrualsStatusService,
        ITimeProvider timeProvider, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput(timeProvider.GetLocalDateNow());

        var statusResult = await accrualsStatusService.GetStatusAsync(input, cancellationToken);

        return statusResult.IsSuccess
            ? TypedResults.Ok(statusResult.Value!)
            : TypedResults.Problem(statusResult.Error!.ToProblemDetails());
    }
}
