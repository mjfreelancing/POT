using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared;

public sealed class OtpStatus : EnrichedEnum<OtpStatus>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly OtpStatus Active = new(1);
    public static readonly OtpStatus Used = new(2);
    public static readonly OtpStatus Invalidated = new(3);
    public static readonly OtpStatus Expired = new(4);

    private OtpStatus(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}