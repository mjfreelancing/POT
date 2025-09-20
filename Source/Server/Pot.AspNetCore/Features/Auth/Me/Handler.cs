using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.App.Features.Auth.Me;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Extensions;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Features.Auth.Me;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(IUserService userService,
        IPermissionService permissionService, HttpContext httpContext, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userId = GetUserId(httpContext);

        if (!userId.HasValue)
        {
            return CreateAuthErrorResult();
        }

        // Can't perform these queries in parallel using TaskHelper.WhenAll() without creating a new scope for each DbContext

        var username = await userService.GetUsernameAsync(userId.Value, cancellationToken);

        if (username is null)
        {
            return CreateAuthErrorResult();
        }

        var permissions = await permissionService.GetPermissionsAsync(userId.Value, cancellationToken);

        return Response.Ok(username, permissions);
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var user = httpContext?.User;

        if (user is null)
        {
            return null;
        }

        var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId.IsNullOrEmpty() || !Guid.TryParse(userId, out var userRowId))
        {
            return null;
        }

        return userRowId;
    }

    private static ProblemHttpResult CreateAuthErrorResult()
    {
        var authError = ProblemDetailsErrorFactory.CreateAuthError("The username or password is invalid.");

        return TypedResults.Problem(authError.ToProblemDetails());
    }
}
