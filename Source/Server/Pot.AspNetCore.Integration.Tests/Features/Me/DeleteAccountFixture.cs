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

namespace Pot.AspNetCore.Integration.Tests.Features.Me;

/// <summary>
/// Proves the contract of deleting your own account.
/// </summary>
public class DeleteAccountFixture : IntegrationAuthFixtureBase
{
    private sealed class ValidationProblem
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];

        public string[] AllMessages()
        {
            return [.. Errors.Values.SelectMany(messages => messages)];
        }
    }

    [Fact]
    public async Task Should_Reject_An_Anonymous_Request()
    {
        using var client = CreateClient();
        using var response = await SendDeleteAsync(client, "someone", SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Unauthorized,
            "the route requires an authenticated caller");
    }

    [Fact]
    public async Task Should_Remove_The_Callers_Account_And_Clear_The_Refresh_Cookie()
    {
        var caller = await CreateEnabledUserAsync("self-delete", "Self Delete");

        var tokens = await LoginAsync(caller);

        using var client = CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await UserExistsAsync(caller.UserRowId)).ShouldBeFalse();

        var tokenStatus = await GetCurrentUserAsync(tokens.AccessToken);
        tokenStatus.ShouldBe(HttpStatusCode.Unauthorized, "the deleted user's access token must be rejected");

        var clearedCookie = GetRefreshCookie(response);
        clearedCookie.ShouldNotBeNull("the response must clear the refresh cookie");
        clearedCookie!.ShouldContain("Max-Age=0");
    }

    [Fact]
    public async Task Should_Refuse_A_Wrong_Password()
    {
        var caller = await CreateEnabledUserAsync("wrong-password-self-delete", "Wrong Password Self Delete");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, caller.Username, "NotMyPassword1!");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.AllMessages().ShouldHaveSingleItem().ShouldBe("The password is incorrect.");
        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_A_Username_That_Does_Not_Match()
    {
        var caller = await CreateEnabledUserAsync("mismatch-self-delete", "Mismatch Self Delete");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, "someone-else", SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);

        problem!.AllMessages().ShouldHaveSingleItem().ShouldBe("The username does not match your account.");
        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_The_Sites_Only_Enabled_Administrator()
    {
        var caller = await CreateAdminUserAsync("only-admin-self-delete", "Only Admin Self Delete");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);
        var errorMessage = problem!.AllMessages().ShouldHaveSingleItem();

        errorMessage.ShouldNotBeNull();
        errorMessage.ShouldContain("only enabled administrator");

        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Refuse_The_Last_Enabled_Platform_Administrator()
    {
        var caller = await CreateAdminUserAsync("last-platform-self-delete", "Last Platform Self Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        // Another enabled administrator keeps the site administrable, so only the platform rule can refuse.
        await CreateUserInSiteAsync(callerWithSite.Site.RowId, "platform-colleague", "Platform Colleague", Role.Admin);

        var tokens = await LoginAsync(caller);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, caller.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await SendDeleteAsync(client, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);
        var errorMessage = problem!.AllMessages().ShouldHaveSingleItem();

        errorMessage.ShouldNotBeNull();
        errorMessage.ShouldContain("last enabled platform administrator");

        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Allow_A_Viewer_To_Delete_Their_Own_Account()
    {
        var caller = await CreateEnabledUserAsync("viewer-self-delete", "Viewer Self Delete");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            "every signed-in user may close their own account");

        (await UserExistsAsync(caller.UserRowId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Allow_An_Administrator_To_Delete_Their_Own_Account_When_A_Peer_Remains()
    {
        var caller = await CreateAdminUserAsync("peer-admin-self-delete", "Peer Admin Self Delete");
        var callerWithSite = await GetUserWithSiteAsync(caller.UserRowId);

        await CreateUserInSiteAsync(callerWithSite.Site.RowId, "peer-admin", "Peer Admin", Role.Admin);

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, caller.Username, SeededPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await UserExistsAsync(caller.UserRowId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Reject_A_Request_Without_The_Confirmation_Values()
    {
        var caller = await CreateEnabledUserAsync("missing-confirmation", "Missing Confirmation");

        using var client = await CreateAuthenticatedClientAsync(caller);
        using var response = await SendDeleteAsync(client, username: string.Empty, password: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The rejection names the missing confirmation values, so it comes from the request validator rather
        // than from the pre-delete checks.
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(TestContext.Current.CancellationToken);
        var rejectedProperties = problem!.Errors.Keys.ToList();

        rejectedProperties.ShouldContain("username");
        rejectedProperties.ShouldContain("password");

        (await UserExistsAsync(caller.UserRowId)).ShouldBeTrue();
    }

    private static Task<HttpResponseMessage> SendDeleteAsync(HttpClient client, string username, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/me")
        {
            Content = JsonContent.Create(new { Username = username, Password = password })
        };

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static string? GetRefreshCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(SetCookieHeader, out var setCookieValues))
        {
            return null;
        }

        return setCookieValues.FirstOrDefault(cookie => cookie.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));
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
