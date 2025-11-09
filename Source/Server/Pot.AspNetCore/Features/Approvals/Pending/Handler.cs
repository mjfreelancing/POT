using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Approvals.Pending;

namespace Pot.AspNetCore.Features.Approvals.Pending;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IGetPendingApprovalsService pendingApprovalsService,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var users = await pendingApprovalsService.GetAllAsync(cancellationToken);

        return Response.Ok(users);
    }
}
