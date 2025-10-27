using AllOverIt.Patterns.Enumeration;
using System.Runtime.CompilerServices;

namespace Pot.Shared.Enumerations;

public sealed class Permission : EnrichedEnum<Permission>
{
    // Note: The enums stored in the database have a max length of 50 characters
    public static readonly Permission SiteManage = new(1, "site:manage");
    public static readonly Permission SiteView = new(2, "site:view");
    public static readonly Permission UserManage = new(3, "user:manage");
    public static readonly Permission UserView = new(4, "user:view");
    public static readonly Permission AccountManage = new(5, "account:manage");
    public static readonly Permission AccountView = new(6, "account:view");
    public static readonly Permission ExpenseManage = new(7, "expense:manage");
    public static readonly Permission ExpenseView = new(8, "expense:view");
    public static readonly Permission IncomeManage = new(9, "income:manage");
    public static readonly Permission IncomeView = new(10, "income:view");
    public static readonly Permission MaintenanceExport = new(11, "maintenance:export");
    public static readonly Permission MaintenanceImport = new(12, "maintenance:import");

    private Permission(int value, [CallerMemberName] string? name = default)
        : base(value, name!)
    {
    }
}
