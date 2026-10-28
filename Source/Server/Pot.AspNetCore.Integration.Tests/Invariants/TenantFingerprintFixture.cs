using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// Proves a difference between two snapshots names the tenant, the table, the row and the value that moved, and that
/// an unchanged tenant produces no difference at all.
/// </summary>
public class TenantFingerprintFixture
{
    private static readonly Guid SiteRowId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Should_Report_No_Difference_When_Nothing_Moved()
    {
        var earlier = Create("Account", "7", "Balance", "100");
        var later = Create("Account", "7", "Balance", "100");

        earlier.FindFirstDifference(later).ShouldBeNull();
        earlier.FindChanges(later).ShouldBeEmpty();
    }

    [Fact]
    public void Should_Name_The_Tenant_Table_Row_And_Column_Of_A_Change()
    {
        var earlier = Create("Account", "7", "Balance", "100");
        var later = Create("Account", "7", "Balance", "250");

        var difference = earlier.FindFirstDifference(later);

        difference.ShouldNotBeNull();
        difference!.ShouldContain(SiteRowId.ToString());
        difference.ShouldContain("table 'Account'");
        difference.ShouldContain("row 7");
        difference.ShouldContain("column 'Balance'");
        difference.ShouldContain("'100'");
        difference.ShouldContain("'250'");
    }

    [Fact]
    public void Should_Report_A_Row_That_Is_Gone_As_A_Removal()
    {
        var earlier = Create("User", "3", "Status", "Enabled");
        var later = Create("User", "4", "Status", "Enabled");

        var changes = earlier.FindChanges(later);

        var removal = changes.Single(change => change.WasRemoved);

        removal.Table.ShouldBe("User");
        removal.RowKey.ShouldBe("3");
        removal.Identify().ShouldBe("User|3");
        removal.Description.ShouldContain("is gone");
    }

    [Fact]
    public void Should_Report_A_Row_That_Appeared_As_Neither_Removal_Nor_Change()
    {
        var earlier = Create("User", "3", "Status", "Enabled");
        var later = Create("User", "4", "Status", "Enabled");

        var appeared = earlier.FindChanges(later).Single(change => change.RowKey == "4");

        appeared.WasRemoved.ShouldBeFalse();
        appeared.Description.ShouldContain("appeared");
    }

    [Fact]
    public void Should_Report_A_Column_That_Appeared()
    {
        var earlier = new TenantFingerprint(SiteRowId, Rows(("Expense", "9", [("Amount", "10")])));
        var later = new TenantFingerprint(SiteRowId, Rows(("Expense", "9", [("Amount", "10"), ("Note", "holiday")])));

        var difference = earlier.FindFirstDifference(later);

        difference.ShouldNotBeNull();
        difference!.ShouldContain("column 'Note'");
        difference.ShouldContain("(column absent)");
        difference.ShouldContain("'holiday'");
    }

    private static TenantFingerprint Create(string table, string rowKey, string column, string value)
    {
        return new TenantFingerprint(SiteRowId, Rows((table, rowKey, [(column, value)])));
    }

    private static Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> Rows(
        params (string Table, string RowKey, (string Column, string Value)[] Columns)[] rows)
    {
        return rows
            .GroupBy(row => row.Table)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>)group.ToDictionary(
                    row => row.RowKey,
                    row => (IReadOnlyDictionary<string, string>)row.Columns.ToDictionary(column => column.Column, column => column.Value),
                    StringComparer.Ordinal),
                StringComparer.Ordinal);
    }
}
