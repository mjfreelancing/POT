using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Sites;

/// <summary>
/// Proves the contract of deleting the caller's own site.
/// </summary>
public class DeleteSiteFixture : IntegrationAuthFixtureBase
{
    private sealed class ValidationProblem
    {
        public ValidationError[] Errors { get; set; } = [];
    }

    private sealed class ValidationError
    {
        public string? PropertyName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    [Fact]
    public async Task Should_Reject_An_Anonymous_Request()
    {
        using var client = CreateClient();
        using var response = await SendDeleteAsync(client, "Site", "someone", SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Unauthorized,
            "the route requires an authenticated caller");
    }

    [Fact]
    public async Task Should_Reject_A_Caller_Without_The_Site_Management_Permission()
    {
        var caller = await CreateEnabledUserAsync("no-permission-site-delete", "No Permission Site Delete");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, "Any Site", caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Forbidden,
            "a user without site:manage must not delete the site");

        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Remove_The_Site_And_Its_Users_And_Leave_Another_Site_Untouched()
    {
        var caller = await CreateAdminUserAsync("site-delete", "Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var colleague = await CreateUserInSiteAsync(callerWithSite.Site.RowId, "site-delete-colleague", "Site Delete Colleague", Role.Viewer);

        var other = await CreateAdminUserAsync("untouched-site", "Untouched Site");

        var tokens = await LoginAsync(caller);

        using var client = CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeFalse();
        (await UserExistsAsync(caller.UserRowId)).ShouldBeFalse();
        (await UserExistsAsync(colleague.UserRowId)).ShouldBeFalse();

        var otherWithSite = await GetUserWithSiteAsync(other.UserRowId);
        (await SiteExistsAsync(otherWithSite.Site.RowId)).ShouldBeTrue("another site must be untouched");
        (await UserExistsAsync(other.UserRowId)).ShouldBeTrue();

        var tokenStatus = await GetCurrentUserAsync(tokens.AccessToken);
        tokenStatus.ShouldBe(HttpStatusCode.Unauthorized, "the deleted administrator's token must be rejected");
    }

    [Fact]
    public async Task Should_Clear_The_Refresh_Cookie()
    {
        var caller = await CreateAdminUserAsync("cookie-site-delete", "Cookie Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        var tokens = await LoginAsync(caller);

        using var client = CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var clearedCookie = response.Headers.TryGetValues(SetCookieHeader, out var values)
            ? values.FirstOrDefault(cookie => cookie.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal))
            : null;

        clearedCookie.ShouldNotBeNull("the response must clear the refresh cookie");
        clearedCookie!.ShouldContain("Max-Age=0");
    }

    [Fact]
    public async Task Should_Refuse_A_Wrong_Password()
    {
        var caller = await CreateAdminUserAsync("wrong-password-site-delete", "Wrong Password Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, caller.Username, "NotMyPassword1!");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.Errors.Single().ErrorMessage.ShouldBe("The password is incorrect.");
        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_A_Site_Name_That_Does_Not_Match()
    {
        var caller = await CreateAdminUserAsync("mismatch-site-delete", "Mismatch Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, "Another Site", caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.Errors.Single().ErrorMessage.ShouldBe("The site name does not match your site.");
        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_A_Username_That_Does_Not_Match()
    {
        var caller = await CreateAdminUserAsync("mismatch-user-site-delete", "Mismatch User Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, "someone-else", SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.Errors.Single().ErrorMessage.ShouldBe("The username does not match your account.");
        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_A_Site_Holding_The_Last_Enabled_Platform_Administrator()
    {
        var caller = await CreateAdminUserAsync("platform-site-delete", "Platform Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, caller.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.Errors.Single().ErrorMessage.ShouldContain("last enabled platform administrator");
        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Allow_Deleting_A_Site_When_The_Platform_Administrator_Is_Elsewhere()
    {
        var caller = await CreateAdminUserAsync("outside-platform-site-delete", "Outside Platform Site Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);
        var platformAdmin = await CreateAdminUserAsync("elsewhere-platform-admin", "Elsewhere Platform Admin");

        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, platformAdmin.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, callerWithSite.Site.Name, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "the platform administrator is outside this site, so the platform stays operable");

        (await SiteExistsAsync(callerWithSite.Site.RowId)).ShouldBeFalse();
    }

    private static Task<HttpResponseMessage> SendDeleteAsync(HttpClient client, string siteName, string username, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/sites/current")
        {
            Content = JsonContent.Create(new { SiteName = siteName, Username = username, Password = password })
        };

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<bool> UserExistsAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Users.AnyAsync(user => user.RowId == userRowId, TestContext.Current.CancellationToken);
    }

    private async Task<bool> SiteExistsAsync(Guid siteRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<SiteEntity>().AnyAsync(site => site.RowId == siteRowId, TestContext.Current.CancellationToken);
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
