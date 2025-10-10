using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Shared;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

// Example queries for each index:
// 
// 1. Password reset for existing user:
//    WHERE UserId = @userId AND Reason = 'PasswordReset' AND ExpiryUtc > @now AND IsUsed = false
// 
// 2. Signup for new user:
//    WHERE Email = @email AND Reason = 'Signup' AND ExpiryUtc > @now AND IsUsed = false
// 
// 3. Find expired records for cleanup:
//    WHERE ExpiryUtc < @now
// 
// 4. Lookup by correlation ID for debugging/tracing:
//    WHERE CorrelationId = @correlationId
// 
// 5. Count recent attempts per user (rate limiting):
//    WHERE UserId = @userId AND CreatedUtc > @timeWindow
// 
// 6. Count recent attempts per email (rate limiting):
//    WHERE Email = @email AND CreatedUtc > @timeWindow
//
[Index("UserId", nameof(Reason), nameof(ExpiryUtc), nameof(IsUsed))]        // 1 - Password reset for existing user
[Index(nameof(Email), nameof(Reason), nameof(ExpiryUtc), nameof(IsUsed))]   // 2 - Signup for new user
[Index(nameof(ExpiryUtc))]                                                  // 3 - Find expired records
[Index(nameof(CorrelationId))]                                              // 4 - Lookup by correlation id
[Index("UserId", nameof(CreatedUtc))]                                       // 5 - Count recent attempts per user
[Index(nameof(Email), nameof(CreatedUtc))]                                  // 6 - Count recent attempts per email
public sealed class OneTimePasswordEntity : EntityBase
{
    // For correlation with telemetry and logs
    [Required]
    [SmallString]
    public required string CorrelationId { get; set; }

    // Required for user when not an existing user
    [Required]
    [MediumString]
    [Citext]
    public required string Email { get; set; }

    [Required]
    public required OtpReason Reason { get; set; }

    [Required]
    [MaxLength(6)]
    [OtpCode]
    public required string OtpCode { get; set; }

    [Required]
    public required bool IsUsed { get; set; }

    [Required]
    public required DateTime CreatedUtc { get; set; }

    [Required]
    public required DateTime ExpiryUtc { get; set; }

    public DateTime? VerifiedUtc { get; set; }

    // Will be null for a new user (signup)
    public UserEntity? User { get; set; }
}