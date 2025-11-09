using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Approvals.UpdateStatus;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Approvals.UpdateStatus.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Approvals.UpdateStatus;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        Request request, IUpdateApprovalService updateApprovalService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var approvalInput = request.MapToInput(id);

        var result = await updateApprovalService.UpdateUserApprovalAsync(approvalInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}
