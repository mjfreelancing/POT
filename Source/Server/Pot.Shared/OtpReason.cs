using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared;

public sealed class OtpReason : EnrichedEnum<OtpReason>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly OtpReason Signup = new(1);
    public static readonly OtpReason PasswordReset = new(2);

    private OtpReason(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
