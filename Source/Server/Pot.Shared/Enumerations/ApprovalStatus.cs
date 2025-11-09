using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class ApprovalStatus : EnrichedEnum<ApprovalStatus>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly ApprovalStatus Approved = new(1);
    public static readonly ApprovalStatus Rejected = new(2);

    private ApprovalStatus(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
