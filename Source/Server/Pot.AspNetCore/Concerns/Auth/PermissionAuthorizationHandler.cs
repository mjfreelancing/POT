using AllOverIt.Assertion;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory.WhenNotNull();
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // return early from this method to simulate a 401 Unauthorized

        var userRowId = context.User.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userRowId, out Guid parsedUserRowId))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        var permissions = await permissionService
            .GetPermissionsAsync(parsedUserRowId, CancellationToken.None)
            .ConfigureAwait(false);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
