using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Users;

/// <summary>
/// Proves the contract of deleting a user in the caller's own site.
/// </summary>
public class DeleteUserFixture : IntegrationAuthFixtureBase
{
    private sealed class ValidationProblem
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];

        public string[] AllMessages()
        {
            return [.. Errors.Values.SelectMany(messages => messages)];
        }
    }

    private sealed class RoleOption
    {
        public Guid RowId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private const string DeleteUserServiceCategory = "Pot.App.Features.Users.Delete.DeleteUserService";

    [Fact]
    public async Task Should_Reject_An_Anonymous_Request()
    {
        var target = await CreateEnabledUserAsync("anonymous-delete-target", "Anonymous Delete Target");

        using var client = CreateClient();
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Unauthorized,
            "the route requires an authenticated caller");
    }

    [Fact]
    public async Task Should_Reject_A_Caller_Without_The_User_Management_Permission()
    {
        var caller = await CreateEnabledUserAsync("no-permission-caller", "No Permission Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "no-permission-target", "No Permission Target");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Forbidden,
            "a user without user:manage must not delete another user");

        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Return_Not_Found_For_A_User_In_Another_Site()
    {
        var caller = await CreateAdminUserAsync("foreign-delete-caller", "Foreign Delete Caller");
        var target = await CreateEnabledUserAsync("foreign-delete-target", "Foreign Delete Target");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.NotFound,
            "a target outside the caller's site must be reported as not found");

        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Remove_The_User_And_Reject_Their_Access_Token()
    {
        var caller = await CreateAdminUserAsync("delete-caller", "Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "delete-target", "Delete Target", Role.Viewer);

        var targetTokens = await LoginAsync(target);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await UserExistsAsync(target.UserRowId)).ShouldBeFalse();

        var tokenStatus = await GetCurrentUserAsync(targetTokens.AccessToken);

        tokenStatus.ShouldBe(
            HttpStatusCode.Unauthorized,
            "the removed user's access token must no longer be accepted");
    }

    [Fact]
    public async Task Should_Remove_A_Pending_User_Who_Never_Activated()
    {
        var caller = await CreateAdminUserAsync("pending-delete-caller", "Pending Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "pending-delete-target", "Pending Delete Target",
            Role.Viewer, UserStatus.Pending);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await UserExistsAsync(target.UserRowId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Remove_A_Disabled_User()
    {
        var caller = await CreateAdminUserAsync("disabled-delete-caller", "Disabled Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "disabled-delete-target", "Disabled Delete Target",
            Role.Viewer, UserStatus.Disabled);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await UserExistsAsync(target.UserRowId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Leave_The_Sites_Accounts_Expenses_And_Incomes_In_Place()
    {
        var caller = await CreateAdminUserAsync("records-left-caller", "Records Left Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "records-left-target", "Records Left Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        var accountRowId = await client.CreateAccountAsync("Records Left Account", 1500.0d);

        var expenseRowId = await client.CreateExpenseAsync(ExpenseRequestFactory.CreateBill(accountRowId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Frequency.Months, 1, 40.0d, AccrualPolicy.None, null));

        var incomeRowId = await client.CreateIncomeAsync(IncomeRequestFactory.CreateIncome(accountRowId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), Frequency.Months, 1, 900.0d));

        using (var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword))
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // The records belong to the site rather than to the user who happened to be signed in when they were entered.
        (await client.GetAccountsAsync()).ShouldContain(account => account.RowId == accountRowId);
        (await client.GetExpensesAsync()).ShouldContain(expense => expense.RowId == expenseRowId);
        (await client.GetIncomesAsync()).ShouldContain(income => income.RowId == incomeRowId);
    }

    [Fact]
    public async Task Should_Allow_The_Same_Username_To_Be_Invited_Again_After_Removal()
    {
        var caller = await CreateAdminUserAsync("username-reuse-caller", "Username Reuse Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var roles = await client.GetFromJsonAsync<RoleOption[]>("/api/roles", TestContext.Current.CancellationToken);
        var viewerRoleId = roles!.Single(role => role.Name == Role.Viewer.Name).RowId;

        var username = $"username-reuse-{Guid.NewGuid():N}";

        using (var inviteResponse = await SendInviteAsync(client, username, viewerRoleId))
        {
            inviteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        var users = await client.GetUsersAsync();
        var invited = users.Single(user => user.Username == username);

        using (var deleteResponse = await SendDeleteAsync(client, invited.RowId, SeededPassword))
        {
            deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // Nothing may outlive the removal that would stop the same name being used again.
        using var secondInviteResponse = await SendInviteAsync(client, username, viewerRoleId);

        secondInviteResponse.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "the removal must leave no record that blocks the username being reused");
    }

    [Fact]
    public async Task Should_Refuse_A_Wrong_Password_Without_Removing_The_User()
    {
        var caller = await CreateAdminUserAsync("wrong-password-caller", "Wrong Password Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "wrong-password-target", "Wrong Password Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, target.UserRowId, "NotMyPassword1!");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.AllMessages().ShouldHaveSingleItem().ShouldBe("The password is incorrect.");
        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_Deleting_Yourself_From_The_User_List()
    {
        var caller = await CreateAdminUserAsync("self-delete-caller", "Self Delete Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, caller.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.AllMessages().ShouldHaveSingleItem().ShouldBe("You cannot delete your own account with this request.");
        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_Removing_The_Last_Enabled_Platform_Administrator()
    {
        var caller = await CreateAdminUserAsync("last-platform-delete-caller", "Last Platform Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "last-platform-delete-target",
            "Last Platform Delete Target", Role.Admin);

        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, target.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);
        var errorMessage = problem!.AllMessages().ShouldHaveSingleItem();

        errorMessage.ShouldNotBeNull();
        errorMessage.ShouldContain("last enabled platform administrator");

        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Allow_Removing_A_Platform_Administrator_When_Another_Remains()
    {
        var caller = await CreateAdminUserAsync("peer-platform-delete-caller", "Peer Platform Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "peer-platform-delete-target",
            "Peer Platform Delete Target", Role.Admin);

        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, caller.UserRowId, target.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "another enabled platform administrator and another enabled site administrator remain");

        (await UserExistsAsync(target.UserRowId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Refuse_Removing_The_Sites_Only_Enabled_Administrator()
    {
        // user:manage is held only by the Admin role today, so an authorised caller is normally a second
        // administrator. This case grants the permission to a Viewer, which is the only way to reach the
        // site-administrability rule through this route, and proves the endpoint wires it and reports it.
        var target = await CreateAdminUserAsync("only-site-admin", "Only Site Admin");
        var targetWithSite = await GetUserWithSiteAsync(target.UserRowId);

        var caller = await CreateUserInSiteAsync(targetWithSite.Site.RowId, "viewer-with-manage", "Viewer With Manage", Role.Viewer);

        await GrantUserManageToViewerRoleAsync();

        try
        {
            using var client = await CreateAuthenticatedClientAsync(caller);
            using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

            problem!.AllMessages().ShouldHaveSingleItem().ShouldBe(
                "This is the site's only enabled administrator. Promote or enable another administrator first.");

            (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
        }
        finally
        {
            await RevokeUserManageFromViewerRoleAsync();
        }
    }

    [Fact]
    public async Task Should_Report_Not_Found_When_The_User_Is_Deleted_Twice()
    {
        var caller = await CreateAdminUserAsync("repeat-delete-caller", "Repeat Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "repeat-delete-target", "Repeat Delete Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        using (var firstResponse = await SendDeleteAsync(client, target.UserRowId, SeededPassword))
        {
            firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var secondResponse = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Reject_A_Request_Without_A_Password()
    {
        var caller = await CreateAdminUserAsync("no-password-caller", "No Password Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "no-password-target", "No Password Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        using var response = await SendDeleteAsync(client, target.UserRowId, password: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The rejection names the missing confirmation value, so it comes from the request validator rather
        // than from the pre-delete checks.
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.Errors.Keys.ShouldContain("password");

        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Report_Every_Repeated_Wrong_Password_As_Invalid()
    {
        var caller = await CreateAdminUserAsync("repeated-password-caller", "Repeated Password Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "repeated-password-target", "Repeated Password Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        // The confirmation adds no attempt counter of its own, so each wrong password is answered the same way.
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var response = await SendDeleteAsync(client, target.UserRowId, "NotMyPassword1!");

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, $"attempt {attempt}");

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

            problem!.AllMessages().ShouldHaveSingleItem().ShouldBe("The password is incorrect.");
        }

        (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_Removing_The_Only_Enabled_Administrator_When_The_Other_Administrator_Is_Disabled()
    {
        var target = await CreateAdminUserAsync("disabled-peer-admin", "Disabled Peer Admin");
        var targetWithSite = await GetUserWithSiteAsync(target.UserRowId);

        // A disabled administrator does not keep the site administrable, so the target is still its only enabled one.
        await CreateUserInSiteAsync(targetWithSite.Site.RowId, "disabled-peer", "Disabled Peer", Role.Admin, UserStatus.Disabled);

        var caller = await CreateUserInSiteAsync(targetWithSite.Site.RowId, "enable-count-caller", "Enable Count Caller", Role.Viewer);

        await GrantUserManageToViewerRoleAsync();

        try
        {
            using var client = await CreateAuthenticatedClientAsync(caller);
            using var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            (await UserExistsAsync(target.UserRowId)).ShouldBeTrue();
        }
        finally
        {
            await RevokeUserManageFromViewerRoleAsync();
        }
    }

    [Fact]
    public async Task Should_Log_The_Removal_Server_Side()
    {
        var caller = await CreateAdminUserAsync("logged-delete-caller", "Logged Delete Caller");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var target = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "logged-delete-target", "Logged Delete Target", Role.Viewer);

        using var client = await CreateAuthenticatedClientAsync(caller);

        using (var response = await SendDeleteAsync(client, target.UserRowId, SeededPassword))
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var scope = CreateScope();
        var collector = scope.ServiceProvider.GetFakeLogCollector();

        var entries = collector.GetSnapshot()
            .Where(record => record.Category == DeleteUserServiceCategory)
            .Where(record => record.Message.Contains(target.UserRowId.ToString(), StringComparison.Ordinal))
            .ToList();

        entries.ShouldNotBeEmpty("each removal is recorded in the server log");
        entries.Any(entry => entry.Message.Contains("Sessions removed", StringComparison.Ordinal))
            .ShouldBeTrue("the record carries what the sweep removed");
    }

    private static Task<HttpResponseMessage> SendDeleteAsync(HttpClient client, Guid userRowId, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{userRowId}")
        {
            Content = JsonContent.Create(new { Password = password })
        };

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static Task<HttpResponseMessage> SendInviteAsync(HttpClient client, string username, Guid roleId)
    {
        var request = new
        {
            Username = username,
            Email = $"{username}@example.com",
            RoleIds = new[] { roleId }
        };

        return client.PostAsJsonAsync("/api/users/invite", request, TestContext.Current.CancellationToken);
    }

    private async Task GrantUserManageToViewerRoleAsync()
    {
        await SetViewerUserManageAsync(grant: true);
    }

    private async Task RevokeUserManageFromViewerRoleAsync()
    {
        await SetViewerUserManageAsync(grant: false);
    }

    private async Task SetViewerUserManageAsync(bool grant)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var viewerRole = await dbContext.Set<RoleEntity>()
            .AsTracking()
            .Include(role => role.Permissions)
            .SingleAsync(role => role.Name == Role.Viewer, TestContext.Current.CancellationToken);

        var userManagePermission = await dbContext.Set<PermissionEntity>()
            .AsNoTracking()
            .SingleAsync(permission => permission.Name == Permission.UserManage, TestContext.Current.CancellationToken);

        var isGranted = viewerRole.Permissions.Any(permission => permission.RowId == userManagePermission.RowId);

        if (grant && !isGranted)
        {
            dbContext.Attach(userManagePermission);
            viewerRole.Permissions.Add(userManagePermission);
        }
        else if (!grant && isGranted)
        {
            var existingPermission = viewerRole.Permissions.Single(permission => permission.RowId == userManagePermission.RowId);
            viewerRole.Permissions.Remove(existingPermission);
        }
        else
        {
            return;
        }

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<bool> UserExistsAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Users.AnyAsync(user => user.RowId == userRowId, TestContext.Current.CancellationToken);
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
}
