using Pot.Data.Entities;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// The entities that belong to no tenant, each with the reason it is shared, so a fingerprint cannot mistake a
/// shared table for an omission.
/// </summary>
/// <remarks>
/// Reaching a site is the test, not naming a table: an entity that can be reached from a site through its foreign
/// keys is tenant-scoped and needs no entry here. An entity that cannot be reached is only accepted once it appears
/// in this list, which is what makes a newly declared table fail until someone decides what it belongs to.
/// </remarks>
public static class GlobalEntities
{
    private static readonly GlobalEntity[] Entities =
    [
        new(
            typeof(RoleEntity),
            "Roles are seeded once for the whole product and shared by every site; a site holds them through its users, not as rows of its own."),
        new(
            typeof(PermissionEntity),
            "Permissions are seeded once for the whole product and granted to roles, so they never belong to a single site.")
    ];

    /// <summary>
    /// The shared entities, each with its reason.
    /// </summary>
    public static IReadOnlyList<GlobalEntity> All
    {
        get
        {
            return Entities;
        }
    }
}
