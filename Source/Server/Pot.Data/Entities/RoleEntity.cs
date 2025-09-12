using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class RoleEntity : EntityBase
    {
        [Required]
        [MediumString]
        public required string Name { get; set; }

        public ICollection<UserEntity> Users { get; set; } = [];                // Skip navigation property (skips join table)
        public ICollection<PermissionEntity> Permissions { get; set; } = [];    // Skip navigation property (skips join table)
    }
}