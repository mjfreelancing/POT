using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared;

public sealed class OtpStatus : EnrichedEnum<OtpStatus>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly OtpStatus Active = new(1);           // A new request, pending verification
    public static readonly OtpStatus Used = new(2);             // A new request that has been verified
    public static readonly OtpStatus Invalidated = new(3);      // A request that has been invalidated by another new request
    public static readonly OtpStatus Expired = new(4);          // A request that has not been verified within a given time period
    public static readonly OtpStatus Failed = new(5);           // A request that failed verification

    private OtpStatus(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}