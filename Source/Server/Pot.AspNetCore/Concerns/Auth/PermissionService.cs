using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Pot.Data.Repositories.Roles;
using System.Data;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class PermissionService : IPermissionService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public PermissionService(IRoleRepository roleRepository, ILogger<PermissionService> logger)
    {
        _roleRepository = roleRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        var roles = await _roleRepository
            .GetRolesForUserAsync(userId, true, cancellationToken)
            .ConfigureAwait(false);

        return [.. roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name.Name)];
    }
}
