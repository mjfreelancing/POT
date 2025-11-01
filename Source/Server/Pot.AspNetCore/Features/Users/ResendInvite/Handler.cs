using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.ResendInvite;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Users.ResendInvite;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Guid id, IResendInviteService resendInviteService,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null, new { id });

        var output = await resendInviteService.ResendInviteAsync(id, cancellationToken);

        return output.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(output.Error!.ToProblemDetails());
    }
}
