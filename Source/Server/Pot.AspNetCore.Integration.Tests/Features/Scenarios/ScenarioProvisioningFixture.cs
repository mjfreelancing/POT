using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Scenarios;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Features.Scenarios;

/// <summary>
/// Proves a declared scenario is provisioned through the routes a person uses, so its people hold the state they
/// were declared with and nothing about them was fabricated.
/// </summary>
public class ScenarioProvisioningFixture : ScenarioFixtureBase
{
    private const string DirectWriteReason = "The product offers no route that disables a site's only administrator";

    [Fact]
    public async Task Should_Provision_A_Tenant_Through_The_Routes()
    {
        var definition = new ScenarioDefinition("provisioned",
        [
            new ScenarioMember("editor", Role.Viewer),
            new ScenarioMember("retired", Role.Viewer, Disabled: true)
        ]);

        var scenario = await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken);

        using var ownerClient = await CreateScenarioClientAsync(scenario.Owner);
        var currentUser = await ownerClient.GetCurrentUserAsync();

        currentUser.Site.RowId.ShouldBe(scenario.SiteRowId);

        var users = await ownerClient.GetUsersAsync();

        var owner = users.Single(user => user.Username == scenario.Owner.Username);
        owner.Status.ShouldBe(UserStatus.Enabled.Name);
        owner.Roles.ShouldContain(Role.Admin.Name);

        var editor = users.Single(user => user.Username == scenario.Members["editor"].Username);
        editor.Status.ShouldBe(UserStatus.Enabled.Name);
        editor.Roles.ShouldContain(Role.Viewer.Name);

        var retired = users.Single(user => user.Username == scenario.Members["retired"].Username);
        retired.Status.ShouldBe(UserStatus.Disabled.Name);
        retired.Roles.ShouldContain(Role.Viewer.Name);

        // The invited person's password came from their invitation, so it has to work for that person alone.
        using var editorClient = await CreateScenarioClientAsync(scenario.Members["editor"]);
        var editorUser = await editorClient.GetCurrentUserAsync();

        editorUser.RowId.ShouldBe(scenario.Members["editor"].UserRowId);
        editorUser.Site.RowId.ShouldBe(scenario.SiteRowId);
    }

    [Fact]
    public async Task Should_Keep_Two_Scenarios_Sharing_A_Name_Apart()
    {
        var definition = new ScenarioDefinition("repeated");

        var first = await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken);
        var second = await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken);

        first.SiteRowId.ShouldNotBe(second.SiteRowId);
        first.Owner.Username.ShouldNotBe(second.Owner.Username);
        first.Owner.UserRowId.ShouldNotBe(second.Owner.UserRowId);

        // Each provisioned tenant holds only the person who signed it up, so neither picked up the other's user.
        (await CountUsersInSiteAsync(first.SiteRowId)).ShouldBe(1);
        (await CountUsersInSiteAsync(second.SiteRowId)).ShouldBe(1);
    }

    [Fact]
    public async Task Should_Apply_A_Declared_State_That_No_Route_Can_Produce()
    {
        var directWrite = new ScenarioDirectWrite(DirectWriteReason, UserStatus.Disabled);
        var definition = new ScenarioDefinition("written", [], directWrite);

        var scenario = await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken);

        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var owner = await dbContext.Set<UserEntity>()
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.RowId == scenario.Owner.UserRowId, TestContext.Current.CancellationToken);

        owner.Status.ShouldBe(UserStatus.Disabled);
    }

    [Fact]
    public async Task Should_Refuse_A_Declared_State_With_No_Stated_Reason()
    {
        var directWrite = new ScenarioDirectWrite("   ", UserStatus.Disabled);
        var definition = new ScenarioDefinition("unexplained", [], directWrite);

        var exception = await Should.ThrowAsync<ScenarioProvisioningException>(async () =>
            await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken));

        exception.Step.ShouldBe("validate the declaration");
        exception.Detail.ShouldContain("why no route can produce the state");
    }

    [Fact]
    public async Task Should_Refuse_A_Declaration_That_Labels_Two_Members_Alike()
    {
        var definition = new ScenarioDefinition("ambiguous",
        [
            new ScenarioMember("reader", Role.Viewer),
            new ScenarioMember("Reader", Role.Admin)
        ]);

        var exception = await Should.ThrowAsync<ScenarioProvisioningException>(async () =>
            await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken));

        exception.Step.ShouldBe("validate the declaration");
        exception.Detail.ShouldContain("reader");
    }

    private async Task<int> CountUsersInSiteAsync(Guid siteRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<UserEntity>()
            .IgnoreQueryFilters()
            .CountAsync(entity => entity.Site.RowId == siteRowId, TestContext.Current.CancellationToken);
    }
}
