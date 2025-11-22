using AllOverIt.Logging.Extensions;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Features.Me.Services;

namespace Pot.AspNetCore.Features.Auth.Logout;

internal sealed class Handler
{
    public static async Task<IResult> Invoke(HttpContext httpContext, IHttpUserService userService,
        IAuthService authService, AuthenticationOptions authOptions, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        try
        {
            var userInfo = await userService.GetMeInfoAsync(httpContext, cancellationToken);

            // Ignore if the user was not found - don't want to provide any hints to the caller
            if (userInfo is not null)
            {
                _ = await authService.LogoutAsync(userInfo.RowId, cancellationToken);
            }
        }
        finally
        {
            // Clear the refresh token cookie
            RefreshTokenHelper.ClearCookie(httpContext, authOptions);
        }

        return Results.Ok();
    }
}
