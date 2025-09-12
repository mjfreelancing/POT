using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    public sealed class UserEntity : EntityBase
    {
        [Required]
        [MediumString]
        [Citext]
        public required string Username { get; set; }

        [Required]
        [MediumString]  // Hash is not a fixed length, but typically a little over 80 - see comment in UserPasswordHasher
        public required string PasswordHash { get; set; }

        [MediumString]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryUtc { get; set; }

        public required SiteEntity Site { get; set; }

        public ICollection<RoleEntity> Roles { get; set; } = [];   // Skip navigation property (skips join table)
    }
}