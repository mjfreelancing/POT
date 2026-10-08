using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Security;

/// <summary>
/// Proves that a caller cannot act on a user or a site that belongs to another site, and that the user
/// mutation routes cannot be used to leave a tenant without an enabled administrator.
/// </summary>
/// <remarks>
/// Each seeded user is placed in its own site, so a seeded pair is a caller in one site and a target in
/// another. A target the caller cannot act on must be reported as not-found, identical to the response for
/// an id that does not exist, so a refusal never discloses that a row exists elsewhere.
/// The tests read the target's current etag from the test database so that each case exercises
/// authorization rather than the concurrency check.
/// </remarks>
public class CrossTenantAuthorizationFixture : IntegrationAuthFixtureBase
{
    private const string UpdatedDisplayName = "Cross Tenant Update";
    private const string UpdatedEmail = "cross-tenant@example.com";
    private const string UpdatedSiteName = "Cross Tenant Site";

    [Fact]
    public async Task Should_Reject_An_Anonymous_Update_Of_A_Users_Details()
    {
        var target = await CreateEnabledUserAsync("anonymous-target", "Anonymous Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = CreateClient();
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(target.UserRowId, targetBefore.Etag),
            TestContext.Current.CancellationToken);

        AssertUnauthorized(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.DisplayName.ShouldBe(targetBefore.DisplayName);
        targetAfter.Email.ShouldBe(targetBefore.Email);
    }

    [Fact]
    public async Task Should_Reject_An_Anonymous_Update_Of_An_Unknown_Users_Details()
    {
        using var client = CreateClient();
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(Guid.NewGuid(), etag: 1),
            TestContext.Current.CancellationToken);

        // Authorization is decided before the target is resolved, so this must not disclose whether the id exists.
        AssertUnauthorized(response);
    }

    [Fact]
    public async Task Should_Not_Change_Another_Sites_User_Details()
    {
        var caller = await CreateAdminUserAsync("details-caller", "Details Caller");
        var target = await CreateEnabledUserAsync("details-target", "Details Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(target.UserRowId, targetBefore.Etag),
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.DisplayName.ShouldBe(targetBefore.DisplayName);
        targetAfter.Email.ShouldBe(targetBefore.Email);
    }

    [Fact]
    public async Task Should_Not_Change_Another_Sites_User_Details_When_The_Caller_Cannot_Manage_Users()
    {
        var caller = await CreateEnabledUserAsync("viewer-caller", "Viewer Caller");
        var target = await CreateEnabledUserAsync("viewer-target", "Viewer Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(target.UserRowId, targetBefore.Etag),
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.DisplayName.ShouldBe(targetBefore.DisplayName);
        targetAfter.Email.ShouldBe(targetBefore.Email);
    }

    [Fact]
    public async Task Should_Not_Change_Another_Sites_User_Status()
    {
        var caller = await CreateAdminUserAsync("status-caller", "Status Caller");
        var target = await CreateEnabledUserAsync("status-target", "Status Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = targetBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{target.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.Status.Name.ShouldBe(targetBefore.Status.Name);
    }

    [Fact]
    public async Task Should_Not_Grant_A_Role_To_Another_Sites_User()
    {
        var caller = await CreateAdminUserAsync("roles-caller", "Roles Caller");
        var target = await CreateEnabledUserAsync("roles-target", "Roles Target");
        var targetBefore = await GetUserAsync(target.UserRowId);
        var adminRoleRowId = await GetRoleRowIdAsync(Role.Admin);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = targetBefore.Etag, RoleIds = new[] { adminRoleRowId } });
        using var response = await client.PutAsync(
            $"/api/users/{target.UserRowId}/roles",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserWithRolesAsync(target.UserRowId);
        targetAfter.Roles.ShouldNotContain(role => role.RowId == adminRoleRowId);
    }

    [Fact]
    public async Task Should_Not_Reset_Another_Sites_User_Password_When_Resending_An_Invite()
    {
        var caller = await CreateAdminUserAsync("resend-caller", "Resend Caller");
        var target = await CreateEnabledUserAsync("resend-target", "Resend Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.PostAsync(
            $"/api/users/{target.UserRowId}/resend-invite",
            content: null,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.PasswordHash.ShouldBe(targetBefore.PasswordHash);
    }

    [Fact]
    public async Task Should_Not_Resend_An_Invitation_For_A_User_Who_Is_Not_Pending()
    {
        // The caller is the target: an enabled administrator in their own site, so the refusal can only
        // come from the target's status and not from the site binding.
        var caller = await CreateAdminUserAsync("resend-enabled", "Resend Enabled");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var response = await client.PostAsync(
            $"/api/users/{caller.UserRowId}/resend-invite",
            content: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.UnprocessableEntity,
            "an invitation can only be re-sent while it is still outstanding");

        var callerAfter = await GetUserAsync(caller.UserRowId);
        callerAfter.PasswordHash.ShouldBe(callerBefore.PasswordHash);
    }

    [Fact]
    public async Task Should_Not_Change_Another_Site()
    {
        var caller = await CreateAdminUserAsync("site-caller", "Site Caller");
        var target = await CreateEnabledUserAsync("site-target", "Site Target");
        var targetUser = await GetUserWithSiteAsync(target.UserRowId);
        var targetSite = targetUser.Site;

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new
        {
            Etag = targetSite.Etag,
            Name = UpdatedSiteName,
            Description = "Cross tenant rewrite"
        });

        using var response = await client.PutAsync(
            $"/api/sites/{targetSite.RowId}",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetSiteAfter = await GetSiteAsync(targetSite.RowId);
        targetSiteAfter.Name.ShouldBe(targetSite.Name);
    }

    [Fact]
    public async Task Should_Allow_A_User_To_Update_Their_Own_Details()
    {
        var caller = await CreateEnabledUserAsync("self-edit", "Self Edit");

        using var client = await CreateAuthenticatedClientAsync(caller);

        // Signing in records the login on the user row, which changes its etag, so read it afterwards.
        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var response = await client.SendAsync(
            CreateUserDetailsRequest(caller.UserRowId, callerBefore.Etag),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var callerAfter = await GetUserAsync(caller.UserRowId);
        callerAfter.DisplayName.ShouldBe(UpdatedDisplayName);
        callerAfter.Email.ShouldBe(UpdatedEmail);
    }

    [Fact]
    public async Task Should_Allow_An_Admin_To_Update_Their_Own_Site()
    {
        var caller = await CreateAdminUserAsync("own-site-caller", "Own Site Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var site = callerWithSite.Site;
        using var content = JsonContent.Create(new
        {
            Etag = site.Etag,
            Name = UpdatedSiteName,
            Description = "Own site rewrite"
        });

        using var response = await client.PutAsync(
            $"/api/sites/{site.RowId}",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var siteAfter = await GetSiteAsync(site.RowId);
        siteAfter.Name.ShouldBe(UpdatedSiteName);
    }

    [Fact]
    public async Task Should_Return_Not_Found_When_Changing_The_Status_Of_A_User_That_Does_Not_Exist()
    {
        var caller = await CreateAdminUserAsync("missing-caller", "Missing Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = 1, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{Guid.NewGuid()}/status",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);
    }

    [Fact]
    public async Task Should_Not_Disable_The_Last_Enabled_Admin()
    {
        var caller = await CreateAdminUserAsync("last-admin-status", "Last Admin Status");

        using var client = await CreateAuthenticatedClientAsync(caller);

        // Signing in records the login on the user row, which changes its etag, so read it afterwards.
        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var content = JsonContent.Create(new { Etag = callerBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{caller.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.UnprocessableEntity,
            "a site must never be left without an enabled administrator");

        var callerAfter = await GetUserAsync(caller.UserRowId);
        callerAfter.Status.Name.ShouldBe(callerBefore.Status.Name);
    }

    [Fact]
    public async Task Should_Not_Remove_Admin_From_The_Last_Enabled_Admin()
    {
        var caller = await CreateAdminUserAsync("last-admin-roles", "Last Admin Roles");

        using var client = await CreateAuthenticatedClientAsync(caller);

        // Signing in records the login on the user row, which changes its etag, so read it afterwards.
        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var content = JsonContent.Create(new { Etag = callerBefore.Etag, RoleIds = Array.Empty<Guid>() });
        using var response = await client.PutAsync(
            $"/api/users/{caller.UserRowId}/roles",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.UnprocessableEntity,
            "a site must never be left without an enabled administrator");

        var adminRoleRowId = await GetRoleRowIdAsync(Role.Admin);
        var callerAfter = await GetUserWithRolesAsync(caller.UserRowId);
        callerAfter.Roles.ShouldContain(role => role.RowId == adminRoleRowId);
    }

    [Fact]
    public async Task Should_Not_Change_A_Colleagues_Details_When_The_Caller_Cannot_Manage_Users()
    {
        // The caller and the colleague share a site, so the row is one the caller can already see on the
        // Users page. Without user:manage the caller may still only edit themselves.
        var caller = await CreateEnabledUserAsync("colleague-caller", "Colleague Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "colleague-target", "Colleague Target");
        var colleagueBefore = await GetUserAsync(colleague.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(colleague.UserRowId, colleagueBefore.Etag),
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var colleagueAfter = await GetUserAsync(colleague.UserRowId);
        colleagueAfter.DisplayName.ShouldBe(colleagueBefore.DisplayName);
        colleagueAfter.Email.ShouldBe(colleagueBefore.Email);
    }

    [Fact]
    public async Task Should_Report_A_Foreign_Target_Indistinguishably_From_A_Missing_One()
    {
        var caller = await CreateAdminUserAsync("indistinguishable-caller", "Indistinguishable Caller");
        var target = await CreateEnabledUserAsync("indistinguishable-target", "Indistinguishable Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);

        using var foreignResponse = await client.PutAsync(
            $"/api/users/{target.UserRowId}/status",
            JsonContent.Create(new { Etag = targetBefore.Etag, Status = nameof(UserStatus.Disabled) }),
            TestContext.Current.CancellationToken);

        using var missingResponse = await client.PutAsync(
            $"/api/users/{Guid.NewGuid()}/status",
            JsonContent.Create(new { Etag = targetBefore.Etag, Status = nameof(UserStatus.Disabled) }),
            TestContext.Current.CancellationToken);

        // Anything that separates the two responses turns the route into a membership oracle.
        foreignResponse.StatusCode.ShouldBe(missingResponse.StatusCode);
        foreignResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        foreignResponse.Content.Headers.ContentType.ShouldBe(missingResponse.Content.Headers.ContentType);

        var foreignBody = await foreignResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        foreignBody.ShouldNotContain(target.UserRowId.ToString(), Case.Insensitive);
        foreignBody.ShouldNotContain(targetBefore.DisplayName, Case.Insensitive);
        foreignBody.ShouldNotContain(targetBefore.Email, Case.Insensitive);
    }

    [Fact]
    public async Task Should_Refuse_A_Foreign_Target_Before_Checking_The_Concurrency_Token()
    {
        var caller = await CreateAdminUserAsync("stale-etag-caller", "Stale Etag Caller");
        var target = await CreateEnabledUserAsync("stale-etag-target", "Stale Etag Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);

        // A deliberately stale etag: the site binding is decided before the concurrency token, so the
        // caller must be told nothing about the row rather than being told their etag is out of date.
        using var content = JsonContent.Create(new
        {
            Etag = targetBefore.Etag + 1,
            Status = nameof(UserStatus.Disabled)
        });

        using var response = await client.PutAsync(
            $"/api/users/{target.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var targetAfter = await GetUserAsync(target.UserRowId);
        targetAfter.Status.Name.ShouldBe(targetBefore.Status.Name);
    }

    [Fact]
    public async Task Should_Log_A_Cross_Tenant_Refusal_At_Warning()
    {
        var caller = await CreateAdminUserAsync("refusal-log-caller", "Refusal Log Caller");
        var target = await CreateEnabledUserAsync("refusal-log-target", "Refusal Log Target");
        var targetBefore = await GetUserAsync(target.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);

        var collector = GetLogCollector();
        collector.Clear();

        using var content = JsonContent.Create(new { Etag = targetBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{target.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        AssertNotFound(response);

        var refusalLog = collector.GetSnapshot()
            .Where(record => record.Level == LogLevel.Warning)
            .Where(record => MentionsRowId(record, target.UserRowId))
            .ToList();

        refusalLog.ShouldNotBeEmpty("a cross-tenant refusal must be logged at warning level");
    }

    [Fact]
    public async Task Should_Not_Disable_A_Configured_Platform_Administrator()
    {
        var caller = await CreateAdminUserAsync("platform-admin", "Platform Admin");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        // A second enabled administrator means the last-enabled-Admin rule cannot be what refuses this, so
        // only the platform-administrator rule can.
        await CreateUserInSiteAsync(callerWithSite.Site.RowId, "platform-admin-second", "Platform Admin Second", Role.Admin);

        // The nominated id has to be in the host's configuration, so this uses a host built for the test.
        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, caller.UserRowId);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var content = JsonContent.Create(new { Etag = callerBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{caller.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.UnprocessableEntity,
            "a configured platform administrator must not be disabled");

        var callerAfter = await GetUserAsync(caller.UserRowId);
        callerAfter.Status.Name.ShouldBe(callerBefore.Status.Name);
    }

    [Fact]
    public async Task Should_Allow_Disabling_An_Admin_When_Another_Enabled_Admin_Remains()
    {
        var caller = await CreateAdminUserAsync("second-admin-caller", "Second Admin Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "second-admin-target", "Second Admin Target", Role.Admin);
        var colleagueBefore = await GetUserAsync(colleague.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = colleagueBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{colleague.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "the site still has an enabled administrator after this change");
    }

    [Fact]
    public async Task Should_Not_Remove_Admin_From_The_Last_Admin_When_Only_A_Viewer_Remains()
    {
        var caller = await CreateAdminUserAsync("viewer-population-caller", "Viewer Population Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        await CreateUserInSiteAsync(callerWithSite.Site.RowId, "viewer-population-colleague", "Viewer Population Colleague", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        // Signing in records the login on the user row, which changes its etag, so read it afterwards.
        var callerBefore = await GetUserAsync(caller.UserRowId);

        using var content = JsonContent.Create(new { Etag = callerBefore.Etag, RoleIds = Array.Empty<Guid>() });
        using var response = await client.PutAsync(
            $"/api/users/{caller.UserRowId}/roles",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.UnprocessableEntity,
            "a Viewer cannot administer the site, so the site would be left without an administrator");

        var adminRoleRowId = await GetRoleRowIdAsync(Role.Admin);
        var callerAfter = await GetUserWithRolesAsync(caller.UserRowId);
        callerAfter.Roles.ShouldContain(role => role.RowId == adminRoleRowId);
    }

    [Fact]
    public async Task Should_Allow_An_Admin_To_Change_A_Colleagues_Details_In_Their_Own_Site()
    {
        var caller = await CreateAdminUserAsync("same-site-details-caller", "Same Site Details Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "same-site-details-target", "Same Site Details Target");
        var colleagueBefore = await GetUserAsync(colleague.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.SendAsync(
            CreateUserDetailsRequest(colleague.UserRowId, colleagueBefore.Etag),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "a user:manage holder may change a colleague in their own site");

        var colleagueAfter = await GetUserAsync(colleague.UserRowId);
        colleagueAfter.DisplayName.ShouldBe(UpdatedDisplayName);
        colleagueAfter.Email.ShouldBe(UpdatedEmail);
    }

    [Fact]
    public async Task Should_Allow_An_Admin_To_Change_A_Colleagues_Status_In_Their_Own_Site()
    {
        var caller = await CreateAdminUserAsync("same-site-status-caller", "Same Site Status Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "same-site-status-target", "Same Site Status Target");
        var colleagueBefore = await GetUserAsync(colleague.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = colleagueBefore.Etag, Status = nameof(UserStatus.Disabled) });
        using var response = await client.PutAsync(
            $"/api/users/{colleague.UserRowId}/status",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "a user:manage holder may change a colleague in their own site");

        var colleagueAfter = await GetUserAsync(colleague.UserRowId);
        colleagueAfter.Status.Name.ShouldBe(nameof(UserStatus.Disabled));
    }

    [Fact]
    public async Task Should_Allow_An_Admin_To_Change_A_Colleagues_Roles_In_Their_Own_Site()
    {
        var caller = await CreateAdminUserAsync("same-site-roles-caller", "Same Site Roles Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "same-site-roles-target", "Same Site Roles Target");
        var colleagueBefore = await GetUserAsync(colleague.UserRowId);
        var viewerRoleRowId = await GetRoleRowIdAsync(Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var content = JsonContent.Create(new { Etag = colleagueBefore.Etag, RoleIds = new[] { viewerRoleRowId } });
        using var response = await client.PutAsync(
            $"/api/users/{colleague.UserRowId}/roles",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "a user:manage holder may change a colleague in their own site");

        var colleagueAfter = await GetUserWithRolesAsync(colleague.UserRowId);
        colleagueAfter.Roles.ShouldContain(role => role.RowId == viewerRoleRowId);
    }

    [Fact]
    public async Task Should_Allow_An_Admin_To_Resend_A_Colleagues_Invitation_In_Their_Own_Site()
    {
        var caller = await CreateAdminUserAsync("same-site-resend-caller", "Same Site Resend Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(
            callerWithSite.Site.RowId,
            "same-site-resend-target",
            "Same Site Resend Target",
            status: UserStatus.Pending);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await client.PostAsync(
            $"/api/users/{colleague.UserRowId}/resend-invite",
            content: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "a user:manage holder may re-send an invitation in their own site");
    }

    private static HttpRequestMessage CreateUserDetailsRequest(Guid userRowId, long etag)
    {
        return new HttpRequestMessage(HttpMethod.Put, $"/api/users/{userRowId}")
        {
            Content = JsonContent.Create(new
            {
                Etag = etag,
                DisplayName = UpdatedDisplayName,
                Email = UpdatedEmail
            })
        };
    }

    private static void AssertUnauthorized(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(
            HttpStatusCode.Unauthorized,
            "the route requires an authenticated caller");
    }

    private static void AssertNotFound(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(
            HttpStatusCode.NotFound,
            "a target the caller cannot act on must be reported as not-found");
    }

    private async Task<UserEntity> GetUserAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.RowId == userRowId, TestContext.Current.CancellationToken);
    }

    private async Task<UserEntity> GetUserWithRolesAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Users
            .AsNoTracking()
            .Include(user => user.Roles)
            .SingleAsync(user => user.RowId == userRowId, TestContext.Current.CancellationToken);
    }

    private async Task<UserEntity> GetUserWithSiteAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Users
            .AsNoTracking()
            .Include(user => user.Site)
            .SingleAsync(user => user.RowId == userRowId, TestContext.Current.CancellationToken);
    }

    private async Task<SiteEntity> GetSiteAsync(Guid siteRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<SiteEntity>()
            .AsNoTracking()
            .SingleAsync(site => site.RowId == siteRowId, TestContext.Current.CancellationToken);
    }

    private async Task<Guid> GetRoleRowIdAsync(Role role)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<RoleEntity>()
            .AsNoTracking()
            .Where(entity => entity.Name == role)
            .Select(entity => entity.RowId)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    private FakeLogCollector GetLogCollector()
    {
        using var scope = CreateScope();

        return scope.ServiceProvider.GetFakeLogCollector();
    }

    private static bool MentionsRowId(FakeLogRecord record, Guid rowId)
    {
        var value = rowId.ToString();

        if (record.Message.Contains(value, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return record.StructuredState?.Any(
            entry => entry.Value?.Contains(value, StringComparison.OrdinalIgnoreCase) == true) == true;
    }
}
