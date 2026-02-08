using Microsoft.EntityFrameworkCore;
using Pot.Shared.Enumerations;

namespace Pot.Data.Entities;

[Index("SiteId", nameof(Category), nameof(Key), IsUnique = true)]
public sealed class SettingEntity : EntityBase
{
    public required SettingCategory Category { get; set; }

    public required string Key { get; set; }
    public required string Value { get; set; }

    public SiteEntity? Site { get; set; }
}
