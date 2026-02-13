using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

// Used to determine how income and expense items are renewed
public sealed class RenewalMode : EnrichedEnum<RenewalMode>
{
    // Note: The enums stored in the database have a max length of 50 characters

    /// <summary>Items that are overdue, or due today, will be renewed.</summary>
    public static readonly RenewalMode Overdue = new(1);

    /// <summary>Items that are in the future will be renewed.</summary>
    public static readonly RenewalMode Future = new(2);

    private RenewalMode(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
