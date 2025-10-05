using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared;

public sealed class SettingCategory : EnrichedEnum<SettingCategory>
{
    public static readonly SettingCategory Backup = new(0);

    private SettingCategory(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
