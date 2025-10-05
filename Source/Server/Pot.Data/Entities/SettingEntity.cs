using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Shared;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index("SiteId", nameof(Category), nameof(Key), IsUnique = true)]
    public sealed class SettingEntity : EntityBase
    {
        public required SettingCategory Category { get; set; }

        public required string Key { get; set; }
        public string? Value { get; set; }

        [Required]
        [MediumString]
        [Citext]
        public required string Description { get; set; }

        public SiteEntity? Site { get; set; }
    }
}
