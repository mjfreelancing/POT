using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class Frequency : EnrichedEnum<Frequency>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly Frequency Days = new(1);
    public static readonly Frequency Weeks = new(2);
    public static readonly Frequency Months = new(3);
    public static readonly Frequency Years = new(4);
    public static readonly Frequency OneTime = new(5);

    private Frequency(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
