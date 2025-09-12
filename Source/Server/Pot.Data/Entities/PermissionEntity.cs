using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class PermissionEntity : EntityBase
    {
        [Required]
        [MediumString]
        public required string Name { get; set; }

        // Not strictly necessary (since not used), but makes it clearer this is part of a many-to-many relationship
        public ICollection<RoleEntity> Roles { get; set; } = [];   // Skip navigation property (skips join table)
    }
}