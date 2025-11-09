using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class UserStatus : EnrichedEnum<UserStatus>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly UserStatus Enabled = new(1);
    public static readonly UserStatus Disabled = new(2);
    public static readonly UserStatus Pending = new(3);
    public static readonly UserStatus Approval = new(4);

    private UserStatus(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
