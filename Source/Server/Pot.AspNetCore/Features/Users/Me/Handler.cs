using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Features.Auth;
using Pot.AspNetCore.Features.Auth.Services;

namespace Pot.AspNetCore.Features.Users.Me;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(HttpContext httpContext,
        IHttpUserService userService, IPermissionService permissionService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInfo = await userService.GetUserInfoAsync(httpContext, cancellationToken);

        if (userInfo is null)
        {
            return AuthUtils.CreateAuthErrorResult();
        }

        var permissions = await permissionService.GetPermissionsAsync(userInfo.UserId, cancellationToken);

        return Response.Ok(userInfo.Username, userInfo.DisplayName, userInfo.Email, permissions);
    }
}
