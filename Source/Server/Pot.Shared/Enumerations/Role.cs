using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class Role : EnrichedEnum<Role>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly Role Admin = new(1);
    public static readonly Role Viewer = new(2);

    private Role(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
