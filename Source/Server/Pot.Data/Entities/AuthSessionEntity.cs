using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pot.Data.Entities;

[Index(nameof(RefreshTokenHash), IsUnique = true)]
[Index(nameof(UserId), nameof(RevokedUtc), nameof(ExpiresUtc))]
[Index(nameof(RevokedUtc), nameof(ExpiresUtc))]
public sealed class AuthSessionEntity : EntityBase
{
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    [Required]
    [MediumString]
    public required string RefreshTokenHash { get; set; }

    public required DateTime CreatedUtc { get; set; }
    public required DateTime ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime? LastSeenUtc { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }
}