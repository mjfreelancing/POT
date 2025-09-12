using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared;

public sealed class Permission : EnrichedEnum<Permission>
{
    public static readonly Permission SiteManage = new(0, "site:manage");
    public static readonly Permission SiteView = new(1, "site:view");
    public static readonly Permission UserManage = new(2, "user:manage");
    public static readonly Permission UserView = new(3, "user:view");
    public static readonly Permission FinanceManage = new(4, "finance:manage");
    public static readonly Permission FinanceView = new(5, "finance:view");

    private Permission(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}

