using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Users;

/// <summary>
/// Proves a site keeps an enabled administrator through the user status and role endpoints, and that the
/// platform-administrator safeguard does not restrict which site roles a configured administrator may hold.
/// </summary>
public class SiteAdministrabilityFixture : IntegrationAuthFixtureBase
{
    private const string OnlyEnabledAdministratorMessage =
        "This is the site's only enabled administrator. Promote or enable another administrator first.";

    private const string RemovingAdminMessage =
        "This is the site's only enabled administrator. Promote another administrator first.";

    private sealed class RoleOption
    {
        public Guid RowId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Should_Refuse_Disabling_Yourself_As_The_Sites_Only_Enabled_Administrator()
    {
        var caller = await CreateAdminUserAsync("self-disable-only-admin", "Self Disable Only Admin");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var currentUser = await client.GetCurrentUserAsync();

        var request = new
        {
            Etag = currentUser.Etag,
            Status = UserStatus.Disabled.Name
        };

        using var response = await client.PutAsJsonAsync($"/api/users/{currentUser.RowId}/status", request,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeValidationProblemAsync();

        problem.Errors.Values.SelectMany(messages => messages).ShouldHaveSingleItem()
            .ShouldBe(OnlyEnabledAdministratorMessage);
    }

    [Fact]
    public async Task Should_Refuse_Removing_Your_Own_Admin_Role_As_The_Sites_Only_Enabled_Administrator()
    {
        var caller = await CreateAdminUserAsync("self-unrole-only-admin", "Self Unrole Only Admin");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var currentUser = await client.GetCurrentUserAsync();

        var request = new
        {
            Etag = currentUser.Etag,
            RoleIds = Array.Empty<Guid>()
        };

        using var response = await client.PutAsJsonAsync($"/api/users/{currentUser.RowId}/roles", request,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeValidationProblemAsync();

        problem.Errors.Values.SelectMany(messages => messages).ShouldHaveSingleItem()
            .ShouldBe(RemovingAdminMessage);
    }

    [Fact]
    public async Task Should_Disable_Yourself_When_Another_Enabled_Administrator_Remains()
    {
        var caller = await CreateAdminUserAsync("self-disable-with-peer", "Self Disable With Peer");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var currentUser = await client.GetCurrentUserAsync();

        await CreateUserInSiteAsync(currentUser.Site.RowId, "self-disable-peer", "Self Disable Peer", Role.Admin);

        var request = new
        {
            Etag = currentUser.Etag,
            Status = UserStatus.Disabled.Name
        };

        using var response = await client.PutAsJsonAsync($"/api/users/{currentUser.RowId}/status", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "another enabled administrator keeps the site administrable, so disabling yourself is allowed");
    }

    [Fact]
    public async Task Should_Allow_Removing_The_Admin_Role_From_A_Platform_Administrator()
    {
        var caller = await CreateAdminUserAsync("platform-role-caller", "Platform Role Caller");

        using var callerClient = await CreateAuthenticatedClientAsync(caller);

        var currentUser = await callerClient.GetCurrentUserAsync();

        var target = await CreateUserInSiteAsync(currentUser.Site.RowId, "platform-role-target",
            "Platform Role Target", Role.Admin);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, target.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            callerClient.DefaultRequestHeaders.Authorization!.Parameter);

        var viewerRoleId = await GetRoleIdAsync(client, Role.Viewer);
        var users = await client.GetUsersAsync();
        var listedTarget = users.Single(user => user.RowId == target.UserRowId);

        var request = new
        {
            Etag = listedTarget.Etag,
            RoleIds = new[] { viewerRoleId }
        };

        using var response = await client.PutAsJsonAsync($"/api/users/{target.UserRowId}/roles", request,
            TestContext.Current.CancellationToken);

        // The platform-administrator safeguard concerns the account and its enabled state, not the site roles it holds.
        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "the platform rule does not restrict which site roles a configured administrator holds");
    }

    private static async Task<Guid> GetRoleIdAsync(HttpClient client, Role role)
    {
        var roles = await client.GetFromJsonAsync<RoleOption[]>("/api/roles", TestContext.Current.CancellationToken);

        return roles!.Single(option => option.Name == role.Name).RowId;
    }
}
