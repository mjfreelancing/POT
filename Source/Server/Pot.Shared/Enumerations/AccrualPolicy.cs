using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class AccrualPolicy : EnrichedEnum<AccrualPolicy>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly AccrualPolicy Automatic = new(1);
    public static readonly AccrualPolicy None = new(2);

    private AccrualPolicy(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
