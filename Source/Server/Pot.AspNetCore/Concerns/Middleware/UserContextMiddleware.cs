using AllOverIt.Extensions;
using Pot.Shared;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Concerns.Middleware;

internal sealed class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserContext userContext)
    {
        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId.IsNotNullOrEmpty() && Guid.TryParse(userId, out var userRowId))
        {
            userContext.SetUser(userRowId);
        }

        await _next(context);
    }
}
