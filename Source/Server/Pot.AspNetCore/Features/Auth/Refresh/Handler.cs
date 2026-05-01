using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Concerns.Auth.Services;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Refresh;

internal sealed class Handler
{
    private const string AuthPrefix = "Bearer ";

    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(HttpContext httpContext,
        [FromHeader(Name = "Authorization")]
        [Description("The access token to be refreshed (optional on initial page load)")]
        string? accessToken,

        IAuthService authService, AuthenticationOptions authOptions, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        // Get refresh token from HTTP-only cookie
        var refreshToken = RefreshTokenHelper.GetFromCookie(httpContext, authOptions);

        if (refreshToken.IsNullOrEmpty())
        {
            return AuthUtils.CreateAuthErrorResult();
        }

        // Access token is optional (may not be present on full page refresh)
        if (!accessToken.IsNullOrEmpty() && accessToken.StartsWith(AuthPrefix, StringComparison.OrdinalIgnoreCase))
        {
            accessToken = accessToken[AuthPrefix.Length..];
        }

        var authTokens = await authService.RefreshAsync(accessToken, refreshToken!, cancellationToken);

        if (!authTokens.IsSuccess)
        {
            return TypedResults.Problem(authTokens.Error!.ToProblemDetails());
        }

        // Set new refresh token as HTTP-only cookie
        RefreshTokenHelper.SetCookie(httpContext, authTokens.Value!.RefreshToken, authOptions);

        // RefreshToken is set as HTTP-only cookie, not in response body
        return Response.Ok(authTokens.Value.AccessToken);
    }
}

