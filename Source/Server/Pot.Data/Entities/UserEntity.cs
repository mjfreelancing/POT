using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Shared.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

[Index(nameof(RowId), nameof(TokenVersion), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = false)]        // Can theoretically be a user of multiple sites since the username is the globally unique logon
public sealed class UserEntity : EntityBase
{
    [Required]
    [MediumString]
    [Citext]
    public required string Username { get; set; }

    [Required]
    [MediumString]
    [Citext]
    public required string Email { get; set; }

    [Required]
    [MediumString]
    public required string DisplayName { get; set; }

    public required UserStatus Status { get; set; }

    [Required]
    [MediumString]  // Hash is not a fixed length, but typically a little over 80 - see comment in UserPasswordHasher
    public required string PasswordHash { get; set; }

    // Incremented each time user logs out or password is changed.
    // Used to invalidate all previously issued access tokens.
    public int TokenVersion { get; set; }

    [MediumString]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryUtc { get; set; }
    public DateTime? LastLoggedInUtc { get; set; }

    public required SiteEntity Site { get; set; }

    public ICollection<AuthSessionEntity> AuthSessions { get; set; } = [];
    public ICollection<OneTimePasswordEntity> OneTimePasswords { get; set; } = [];
    public ICollection<RoleEntity> Roles { get; set; } = [];   // Skip navigation property (skips join table)
}

