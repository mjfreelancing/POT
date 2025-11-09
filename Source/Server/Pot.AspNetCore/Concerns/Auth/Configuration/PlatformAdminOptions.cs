using AllOverIt.Extensions;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Configuration for platform-level administrators who have elevated permissions
// outside the normal database-backed role system.
public sealed class PlatformAdminOptions
{
    private Guid[] _userRowIds = [];

    // Comma-separated list of user GUIDs (RowIds) that should have platform-level admin access.
    // These users will have the platform:manage permission added to their normal permissions.
    public string? UserIds { get; set; }

    // Gets the list of platform admin user IDs as a HashSet for efficient lookup.
    public Guid[] GetUserRowIds()
    {
        if (UserIds.IsNotNullOrEmpty())
        {
            _userRowIds = [.. UserIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)];
        }

        return _userRowIds;
    }
}
