using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class SettingCategory : EnrichedEnum<SettingCategory>
{
    // Note: The enums stored in the database have a max length of 50 characters
    // public static readonly SettingCategory Backup = new(1);

    private SettingCategory(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
