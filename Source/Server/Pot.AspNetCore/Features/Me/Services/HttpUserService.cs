using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Pot.App.Features.Me;
using Pot.App.Features.Me.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Features.Me.Services;

internal sealed class HttpUserService : IHttpUserService
{
    private readonly IUserService _userService;

    public HttpUserService(IUserService userService)
    {
        _userService = userService.WhenNotNull();
    }

    public async Task<Output?> GetMeInfoAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var userId = GetMeId(httpContext);

        if (!userId.HasValue)
        {
            return null;
        }

        return await _userService
            .GetUserInfoAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);
    }

    private static Guid? GetMeId(HttpContext httpContext)
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
