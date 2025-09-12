using Microsoft.EntityFrameworkCore;
using Pot.Data.Repositories.Users;
using System.Data;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;

    public PermissionService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        var roles = await _userRepository.Current
            .Include(user => user.Roles)
            .ThenInclude(role => role.Permissions)
            .Where(user => user.RowId == userId)
            .Select(x => x.Roles)
            .ToArrayAsync()
            .ConfigureAwait(false);

        return [.. roles
            .SelectMany(role => role)
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)];
    }
}
