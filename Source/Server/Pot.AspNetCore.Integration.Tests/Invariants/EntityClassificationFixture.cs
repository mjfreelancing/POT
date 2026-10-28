using Microsoft.Extensions.DependencyInjection;
using Pot.Data;
using Pot.Data.Entities;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// Proves the classification rules themselves, and that the model's own entities are all classified.
/// </summary>
/// <remarks>
/// The rules are exercised on types the model does not currently contain, so a table that stops reaching a site, or a
/// shared declaration that a foreign key contradicts, stays reported rather than silently covered.
/// </remarks>
public class EntityClassificationFixture : IntegrationFixtureBase
{
    [Fact]
    public void Should_Report_An_Entity_That_Reaches_No_Site_And_Is_Not_Declared_Shared()
    {
        var scopes = new TenantScopes([typeof(SiteEntity), typeof(string)], [typeof(SiteEntity)], []);

        var problems = EntityClassification.FindProblems(scopes, []);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain(nameof(String));
        problems[0].ShouldContain("neither reaches a site nor is declared shared");
    }

    [Fact]
    public void Should_Report_A_Shared_Entity_That_States_No_Reason()
    {
        var scopes = new TenantScopes([typeof(SiteEntity), typeof(RoleEntity)], [typeof(SiteEntity)], []);

        var problems = EntityClassification.FindProblems(scopes, [new GlobalEntity(typeof(RoleEntity), "  ")]);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain(nameof(RoleEntity));
        problems[0].ShouldContain("without a stated reason");
    }

    [Fact]
    public void Should_Report_A_Shared_Entity_That_Now_Reaches_A_Site()
    {
        var scopes = new TenantScopes([typeof(AccountEntity)], [typeof(AccountEntity)], []);

        var problems = EntityClassification.FindProblems(scopes, [new GlobalEntity(typeof(AccountEntity), "Shared for now.")]);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain(nameof(AccountEntity));
        problems[0].ShouldContain("now reaches a site");
    }

    [Fact]
    public void Should_Report_A_Shared_Entity_The_Model_No_Longer_Declares()
    {
        var scopes = new TenantScopes([typeof(SiteEntity)], [typeof(SiteEntity)], []);

        var problems = EntityClassification.FindProblems(scopes, [new GlobalEntity(typeof(RoleEntity), "Seeded once.")]);

        problems.ShouldHaveSingleItem();
        problems[0].ShouldContain(nameof(RoleEntity));
        problems[0].ShouldContain("no longer declares it");
    }

    [Fact]
    public void Should_Accept_Tenant_Scoped_Entities_And_Shared_Ones_With_A_Reason()
    {
        var scopes = new TenantScopes([typeof(SiteEntity), typeof(RoleEntity)], [typeof(SiteEntity)], []);

        var problems = EntityClassification.FindProblems(scopes, [new GlobalEntity(typeof(RoleEntity), "Seeded once.")]);

        problems.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Classify_Every_Entity_The_Model_Declares()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var scopes = TenantScopeResolver.Resolve(dbContext.Model);

        scopes.Tables.ShouldNotBeEmpty("the model declares tenant-scoped tables");

        var problems = EntityClassification.FindProblems(scopes, GlobalEntities.All);

        problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Should_Reach_The_Site_From_Every_Tenant_Scoped_Table()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var scopes = TenantScopeResolver.Resolve(dbContext.Model);

        var tables = scopes.Tables.Select(table => table.Table).ToList();

        // Every table a tenant's rows live in has to be read, or a deletion could move rows the fingerprint never sees.
        tables.ShouldContain("Site");
        tables.ShouldContain("User");
        tables.ShouldContain("Account");
        tables.ShouldContain("Expense");
        tables.ShouldContain("Income");
        tables.ShouldContain("Setting");
        tables.ShouldContain("AuthSession");
    }
}
