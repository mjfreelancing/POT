using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.App.Features.Me.Delete;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth;

namespace Pot.AspNetCore.Features.Me.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(HttpContext httpContext, [FromBody] Request request,
        IDeleteOwnAccountService accountService, PlatformAdminOptions platformAdminOptions,
        AuthenticationOptions authOptions, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput(platformAdminOptions.GetUserRowIds());

        var result = await accountService.DeleteAccountAsync(input, cancellationToken);

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(result.Error!.ToProblemDetails());
        }

        // The session rows are gone with the user, so the cookie is cleared to stop the client presenting a
        // refresh token for a session that no longer exists.
        RefreshTokenHelper.ClearCookie(httpContext, authOptions);

        return TypedResults.Ok();
    }
}
