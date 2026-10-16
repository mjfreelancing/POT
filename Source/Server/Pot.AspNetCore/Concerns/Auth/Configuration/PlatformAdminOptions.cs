using AllOverIt.Extensions;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Configuration for platform-level administrators who have elevated permissions
// outside the normal database-backed role system.
/// <summary>Provides the configured platform administrator user identifiers.</summary>
public sealed class PlatformAdminOptions
{
    private Guid[] _userRowIds = [];

    // Comma-separated list of user GUIDs (RowIds) that should have platform-level admin access.
    // These users will have the platform:manage permission added to their normal permissions.
    /// <summary>Gets or sets the comma-separated platform administrator user identifiers.</summary>
    public string? UserIds { get; set; }

    // Gets the list of platform admin user IDs as a HashSet for efficient lookup.
    /// <summary>Gets the configured platform administrator user identifiers.</summary>
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
