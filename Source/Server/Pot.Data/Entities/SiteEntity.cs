using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

[Index(nameof(Description), IsUnique = true)]
public sealed class SiteEntity : EntityBase
{
    [Required]
    [MediumString]
    [Citext]
    public required string Description { get; set; }

    public ICollection<UserEntity> Users { get; set; } = [];
    public ICollection<AccountEntity> Accounts { get; set; } = [];
    public ICollection<SettingEntity> Settings { get; set; } = [];
}