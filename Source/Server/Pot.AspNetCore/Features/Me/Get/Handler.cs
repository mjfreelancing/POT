using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Features.Auth;
using Pot.AspNetCore.Features.Me.Services;

namespace Pot.AspNetCore.Features.Me.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(HttpContext httpContext,
        IHttpUserService userService, IPermissionService permissionService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInfo = await userService.GetMeInfoAsync(httpContext, cancellationToken);

        if (userInfo is null)
        {
            return AuthUtils.CreateAuthErrorResult();
        }

        var permissions = await permissionService.GetPermissionsAsync(userInfo.RowId, cancellationToken);

        return Response.Ok(userInfo, permissions);
    }
}
