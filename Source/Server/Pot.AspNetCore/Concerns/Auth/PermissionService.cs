using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Repositories.Users;
using System.Data;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    public PermissionService(IUserRepository userRepository, ILogger<PermissionService> logger)
    {
        _userRepository = userRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { userId });

        var roles = await _userRepository.Users
            .Include(user => user.Roles)
            .ThenInclude(role => role.Permissions)
            .Where(user => user.RowId == userId)
            .Select(user => user.Roles)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. roles
            .SelectMany(role => role)
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name.Name)];
    }
}
