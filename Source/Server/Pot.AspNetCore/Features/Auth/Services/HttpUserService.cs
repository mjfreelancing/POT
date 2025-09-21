using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Pot.App.Features.Auth.Me;
using Pot.AspNetCore.Features.Auth.Services.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Features.Auth.Services;

internal sealed class HttpUserService : IHttpUserService
{
    private readonly IUserService _userService;

    public HttpUserService(IUserService userService)
    {
        _userService = userService.WhenNotNull();
    }

    public async Task<UserInfo?> GetUserInfoAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        if (!userId.HasValue)
        {
            return null;
        }

        var username = await _userService
            .GetUsernameAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return username is null ? null : new UserInfo(userId.Value, username);
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
}
