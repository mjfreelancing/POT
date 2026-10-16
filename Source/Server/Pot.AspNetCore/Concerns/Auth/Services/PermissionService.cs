using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.Data.Repositories.Roles;
using System.Data;

namespace Pot.AspNetCore.Concerns.Auth.Services;

internal sealed class PermissionService : IPermissionService
{
    private readonly IRoleRepository _roleRepository;
    private readonly PlatformAdminOptions _platformAdminOptions;
    private readonly ILogger _logger;

    public PermissionService(IRoleRepository roleRepository, PlatformAdminOptions platformAdminOptions, ILogger<PermissionService> logger)
    {
        _roleRepository = roleRepository.WhenNotNull();
        _platformAdminOptions = platformAdminOptions.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        // Get normal database permissions for the user
        // Cross-tenant by design: permission resolution runs during UseAuthorization, before the user context
        // middleware has established which site the caller is acting in.
        var roles = await _roleRepository.GetRolesForUserAsync(userId, true, cancellationToken);

        var permissions = roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name.Name)
            .ToHashSet();

        // Check if current user is a platform admin (from environment config)
        // If so, add the special platform:manage permission on top of their normal permissions
        var platformAdminIds = _platformAdminOptions.GetUserRowIds();

        if (platformAdminIds.Contains(userId))
        {
            _logger.LogInformation(
                "User with ID '{UserId}' identified as platform admin - granting platform:manage permission",
                userId);

            // Add platform-level permission on top of existing permissions
            permissions.Add(AuthorizationPolicies.PlatformManage);
        }

        return permissions;
    }
}
