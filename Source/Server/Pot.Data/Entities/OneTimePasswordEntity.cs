using Microsoft.EntityFrameworkCore;
using Pot.Data.Annotations;
using Pot.Shared;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities;

// Actual repository query patterns:
// 
// 1. GetPendingExpiredAsync - Find expired OTPs for cleanup:
//    WHERE Status = 'Active' AND ExpiryUtc <= @now [AND Reason = @reason (optional)]
// 
// 2. CountFailedRequestsForUsernameAsync - Rate limiting (account-level):
//    WHERE Username = @username AND Status = 'Failed' AND CreatedUtc >= @afterDate [AND Reason = @reason (optional)]
// 
// 3. GetActiveRequestsForUsernameAsync - Invalidate previous active OTPs:
//    WHERE Username = @username AND Status = 'Active' [AND Reason = @reason (optional)]
// 
// 4. GetRequestsForUsernameAndRefCodeAsync - Verify OTP:
//    WHERE Reason = @reason AND Username = @username AND RefCode = @refCode
//
// Note: Reason is optional in methods 1-3, so indexes cannot rely on Reason as leading column


[Index(nameof(Status), nameof(ExpiryUtc))]  // GetPendingExpiredAsync (with or without Reason filter)
[Index(nameof(Username), nameof(Status), nameof(CreatedUtc))]  // CountFailedRequestsForUsernameAsync + GetActiveRequestsForUsernameAsync
[Index(nameof(Reason), nameof(Username), nameof(RefCode))]  // GetRequestsForUsernameAndRefCodeAsync (Reason is required)
[Index(nameof(ExpiryUtc))]  // General expiry cleanup queries
public sealed class OneTimePasswordEntity : EntityBase
{
    // For correlation with telemetry and logs
    [Required]
    [SmallString]
    public required string CorrelationId { get; set; }

    // Username and Email is required for when not an existing user (sign up) and
    // we need the Username because an email can be associated with multiple sites.

    [Required]
    [MediumString]
    [Citext]
    public required string Username { get; set; }

    [Required]
    [MediumString]
    [Citext]
    public required string Email { get; set; }

    public required OtpReason Reason { get; set; }

    [Required]
    [MaxLength(6)]
    [OtpCode]
    public required string RefCode { get; set; }

    [Required]
    [MaxLength(6)]
    [OtpCode]
    public required string OtpCode { get; set; }

    public int AttemptCount { get; set; } = 0;

    public required OtpStatus Status { get; set; }

    [MediumString]  // Hash is not a fixed length, but typically a little over 80 - see comment in UserPasswordHasher
    public string? TempPasswordHash { get; set; }

    public required DateTime CreatedUtc { get; set; }
    public required DateTime ExpiryUtc { get; set; }
    public DateTime? VerifiedUtc { get; set; }

    // Will be null for a new user (signup)
    public UserEntity? User { get; set; }
}
