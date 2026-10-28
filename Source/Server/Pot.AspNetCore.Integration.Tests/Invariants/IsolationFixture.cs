using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.AspNetCore.Integration.Tests.Invariants.Support;
using Pot.AspNetCore.Integration.Tests.Scenarios;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// Proves each destructive action touches only what it names: another tenant's rows are untouched, the rows the action
/// did not target are untouched, and nothing is left pointing at a row that no longer exists.
/// </summary>
/// <remarks>
/// Every case provisions a canary tenant that the action must not see, and a tenant it acts in. Both are fingerprinted
/// before and after, so a difference is reported as the tenant, the table, the row and the value that moved rather
/// than as a count.
/// </remarks>
public class IsolationFixture : ScenarioFixtureBase
{
    [Fact]
    public async Task Should_Leave_Every_Other_Tenant_And_Row_When_A_Member_Is_Removed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var canary = await ProvisionTenantAsync("isolation-canary", [], cancellationToken);

        var target = await ProvisionTenantAsync("isolation-target",
            [new ScenarioMember("removed", Role.Viewer)], cancellationToken);

        // The caller signs in before the snapshot so the session it creates is part of what is compared.
        using var ownerClient = await CreateScenarioClientAsync(target.Owner);

        var removed = target.Members["removed"];
        var removedUserId = await GetInternalUserIdAsync(removed.UserRowId, cancellationToken);

        var canaryBefore = await CaptureAsync(canary.SiteRowId, cancellationToken);
        var targetBefore = await CaptureAsync(target.SiteRowId, cancellationToken);

        // Removing a person removes the rows that are theirs: their role links and their sessions.
        var allowedRemovals = RowsBelongingTo(targetBefore, removedUserId);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{removed.UserRowId}")
        {
            Content = JsonContent.Create(new { Password = target.Owner.Password })
        };

        using var response = await ownerClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            $"removing a member of the caller's own tenant should succeed, but the API answered {(int)response.StatusCode}: {body}");

        await AssertIsolationAsync(canaryBefore, canary.SiteRowId, targetBefore, target.SiteRowId, allowedRemovals,
            cancellationToken);
    }

    [Fact]
    public async Task Should_Leave_Every_Other_Tenant_And_Row_When_The_Caller_Removes_Their_Own_Account()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var canary = await ProvisionTenantAsync("self-canary", [], cancellationToken);

        // Two administrators, so the caller may remove themselves and the tenant is left administrable.
        var target = await ProvisionTenantAsync("self-target", [new ScenarioMember("peer", Role.Admin)], cancellationToken);

        var peer = target.Members["peer"];

        using var peerClient = await CreateScenarioClientAsync(peer);

        var peerUserId = await GetInternalUserIdAsync(peer.UserRowId, cancellationToken);

        var canaryBefore = await CaptureAsync(canary.SiteRowId, cancellationToken);
        var targetBefore = await CaptureAsync(target.SiteRowId, cancellationToken);

        // Removing the caller removes the rows that are theirs: their role links and their sessions.
        var allowedRemovals = RowsBelongingTo(targetBefore, peerUserId);

        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/me")
        {
            Content = JsonContent.Create(new { Username = peer.Username, Password = peer.Password })
        };

        using var response = await peerClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            $"an administrator should be able to remove their own account while another remains, but the API answered {(int)response.StatusCode}: {body}");

        await AssertIsolationAsync(canaryBefore, canary.SiteRowId, targetBefore, target.SiteRowId, allowedRemovals,
            cancellationToken);
    }

    [Fact]
    public async Task Should_Leave_Every_Other_Tenant_When_A_Site_Is_Removed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var canary = await ProvisionTenantAsync("site-canary", [], cancellationToken);
        var target = await ProvisionTenantAsync("site-target", [], cancellationToken);

        using var ownerClient = await CreateScenarioClientAsync(target.Owner);

        var currentUser = await ownerClient.GetCurrentUserAsync();
        var siteName = currentUser.Site.Name;

        siteName.ShouldNotBeNullOrWhiteSpace("the tenant's name is part of the request that removes it");

        var canaryBefore = await CaptureAsync(canary.SiteRowId, cancellationToken);
        var targetBefore = await CaptureAsync(target.SiteRowId, cancellationToken);

        // Removing the tenant is allowed to remove every row of that tenant, and only those.
        var allowedRemovals = AllRowKeys(targetBefore);

        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/sites/current")
        {
            Content = JsonContent.Create(new
            {
                SiteName = siteName,
                Username = target.Owner.Username,
                Password = target.Owner.Password
            })
        };

        using var response = await ownerClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            $"removing the caller's own tenant should succeed, but the API answered {(int)response.StatusCode}: {body}");

        var targetAfter = await AssertIsolationAsync(canaryBefore, canary.SiteRowId, targetBefore, target.SiteRowId,
            allowedRemovals, cancellationToken);

        targetAfter.Tables.Values.ShouldAllBe(
            rows => rows.Count == 0,
            "the removed tenant must leave no row behind in any table that belongs to a site");
    }

    /// <summary>
    /// Compares both tenants after a destructive action, and confirms nothing is left referencing a removed row.
    /// </summary>
    /// <param name="canaryBefore">The untouched tenant, captured before the action.</param>
    /// <param name="canarySiteRowId">The untouched tenant's identifier.</param>
    /// <param name="targetBefore">The tenant the action acted in, captured before the action.</param>
    /// <param name="targetSiteRowId">The tenant the action acted in.</param>
    /// <param name="allowedRemovals">The rows the action is allowed to have removed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The acting tenant's fingerprint after the action.</returns>
    private async Task<TenantFingerprint> AssertIsolationAsync(TenantFingerprint canaryBefore, Guid canarySiteRowId,
        TenantFingerprint targetBefore, Guid targetSiteRowId, IReadOnlySet<string> allowedRemovals,
        CancellationToken cancellationToken)
    {
        var canaryAfter = await CaptureAsync(canarySiteRowId, cancellationToken);
        var targetAfter = await CaptureAsync(targetSiteRowId, cancellationToken);

        var canaryDifference = canaryBefore.FindFirstDifference(canaryAfter);

        canaryDifference.ShouldBeNull($"another tenant must be untouched by the action: {canaryDifference}");

        var unexpected = targetBefore.FindChanges(targetAfter)
            .Where(change => !change.WasRemoved || !allowedRemovals.Contains(change.Identify()))
            .Select(change => change.Description)
            .ToList();

        unexpected.ShouldBeEmpty(
            $"the action may only remove the rows it targets:{Environment.NewLine}{string.Join(Environment.NewLine, unexpected)}");

        var orphans = await FindOrphansAsync(cancellationToken);

        orphans.ShouldBeEmpty(
            $"no row may outlive what it references:{Environment.NewLine}{string.Join(Environment.NewLine, orphans)}");

        return targetAfter;
    }

    private async Task<ProvisionedScenario> ProvisionTenantAsync(string name, ScenarioMember[] members,
        CancellationToken cancellationToken)
    {
        var definition = new ScenarioDefinition(name, members);
        var scenario = await ProvisionScenarioAsync(definition, cancellationToken);

        using var ownerClient = await CreateScenarioClientAsync(scenario.Owner);

        var accountRowId = await ownerClient.CreateAccountAsync($"{name} account", 1_000.0d);
        var nextDue = GetSiteLocalDateToday();

        var expenseRequest = ExpenseRequestFactory.CreateBill(accountRowId, nextDue, Frequency.OneTime, 0, 25.0d,
            AccrualPolicy.None, null);

        await ownerClient.CreateExpenseAsync(expenseRequest);

        var incomeRequest = IncomeRequestFactory.CreateIncome(accountRowId, nextDue, Frequency.OneTime, 0, 500.0d);

        await ownerClient.CreateIncomeAsync(incomeRequest);

        return scenario;
    }

    private async Task<TenantFingerprint> CaptureAsync(Guid siteRowId, CancellationToken cancellationToken)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await TenantSnapshotter.CaptureAsync(dbContext, siteRowId, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> FindOrphansAsync(CancellationToken cancellationToken)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await OrphanSweep.FindAsync(dbContext, cancellationToken);
    }

    private async Task<int> GetInternalUserIdAsync(Guid userRowId, CancellationToken cancellationToken)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var user = await dbContext.Set<UserEntity>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.RowId == userRowId, cancellationToken);

        return user.Id;
    }

    /// <summary>
    /// Finds the rows that belong to one person: the person's own row, the rows keyed by it — such as a role link — and
    /// the rows that name it, such as a session.
    /// </summary>
    /// <param name="fingerprint">The tenant as it was before the action.</param>
    /// <param name="userId">The person's internal identifier.</param>
    /// <returns>The rows the removal of that person is allowed to remove.</returns>
    private static HashSet<string> RowsBelongingTo(TenantFingerprint fingerprint, int userId)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var owner = userId.ToString(CultureInfo.InvariantCulture);

        foreach (var table in fingerprint.Tables)
        {
            foreach (var row in table.Value)
            {
                // A link table keys the row by the users and roles it joins, so the key is checked part by part as
                // well as the columns that name the person.
                var keyNamesThePerson = row.Key.Split('|').Contains(owner, StringComparer.Ordinal);

                var columnNamesThePerson = row.Value.Any(column =>
                    column.Key.Contains("User", StringComparison.OrdinalIgnoreCase) && column.Value == owner);

                if (keyNamesThePerson || columnNamesThePerson)
                {
                    keys.Add($"{table.Key}|{row.Key}");
                }
            }
        }

        return keys;
    }

    private static HashSet<string> AllRowKeys(TenantFingerprint fingerprint)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var table in fingerprint.Tables)
        {
            foreach (var row in table.Value)
            {
                keys.Add($"{table.Key}|{row.Key}");
            }
        }

        return keys;
    }
}
