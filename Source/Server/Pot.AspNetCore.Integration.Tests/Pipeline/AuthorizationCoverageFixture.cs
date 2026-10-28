using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Proves the coverage rules themselves: an operation nobody classified is reported, an ignore without a reason is
/// reported, and a resource that names a row without declaring one is reported.
/// </summary>
/// <remarks>
/// The rules are exercised on operations the application does not contain, so the failure modes stay proven even
/// while the contract happens to be fully covered.
/// </remarks>
public class AuthorizationCoverageFixture
{
    private static readonly EndpointOperation Documented = EndpointOperation.Create("GET", "/api/accounts");
    private static readonly EndpointOperation Undecided = EndpointOperation.Create("post", "/api/widgets");
    private static readonly EndpointOperation Ignored = EndpointOperation.Create("DELETE", "/api/sites/current");

    [Fact]
    public void Should_Report_An_Operation_That_Is_Neither_Exercised_Nor_Ignored()
    {
        var ignored = new[]
        {
            new IgnoredOperation(Ignored, "Removes the tenant the run acts in.")
        };

        var problems = InventoryCoverage.FindProblems([Documented, Ignored, Undecided], [Documented], ignored);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain("POST /api/widgets");
        problems[0].ShouldContain("neither exercised nor ignored");
    }

    [Fact]
    public void Should_Report_An_Ignored_Operation_That_States_No_Reason()
    {
        var ignored = new[]
        {
            new IgnoredOperation(Ignored, "   ")
        };

        var problems = InventoryCoverage.FindProblems([Documented, Ignored], [Documented], ignored);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain("DELETE /api/sites/current");
        problems[0].ShouldContain("without a stated reason");
    }

    [Fact]
    public void Should_Report_An_Ignore_For_An_Operation_The_Contract_Does_Not_Document()
    {
        var ignored = new[]
        {
            new IgnoredOperation(Undecided, "Left alone until the resource exists.")
        };

        var problems = InventoryCoverage.FindProblems([Documented], [Documented], ignored);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain("POST /api/widgets");
        problems[0].ShouldContain("not a documented operation");
    }

    [Fact]
    public void Should_Report_A_Row_Naming_Operation_Whose_Resource_Is_Not_Classified()
    {
        var rows = new ForeignTenantRows(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var unclassified = rows.FindUnclassifiedOperations([Documented, EndpointOperation.Create("PUT", "/api/widgets/{id}")]);

        unclassified.ShouldHaveSingleItem();
        unclassified[0].ShouldContain("PUT /api/widgets/{id}");
        unclassified[0].ShouldContain("no row of that resource is declared");
    }

    [Fact]
    public void Should_Accept_An_Operation_That_Is_Exercised_And_One_That_Is_Ignored_With_A_Reason()
    {
        var ignored = new[]
        {
            new IgnoredOperation(Ignored, "Removes the tenant the run acts in.")
        };

        var problems = InventoryCoverage.FindProblems([Documented, Ignored], [Documented], ignored);

        problems.ShouldBeEmpty();
    }
}
