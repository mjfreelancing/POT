using Microsoft.EntityFrameworkCore;
using Pot.Shared;

namespace Pot.Data.Entities;

[Index(nameof(Name), IsUnique = true)]
public sealed class RoleEntity : EntityBase
{
    public required Role Name { get; set; }

    public ICollection<UserEntity> Users { get; set; } = [];                // Skip navigation property (skips join table)
    public ICollection<PermissionEntity> Permissions { get; set; } = [];    // Skip navigation property (skips join table)
}