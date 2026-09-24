using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Maintenance.Export;

public class ExportFixture : IntegrationFixtureBase
{
    private const string ExportPath = "/api/maintenance/export";
    private const string LoginPath = "/api/auth/login";
    private const string LoginSuccessStatus = "Success";

    private sealed class LoginResponse
    {
        public string? Status { get; set; }
        public string? AccessToken { get; set; }
    }

    // No token is sent, so this covers the authorization boundary without any data setup.
    [Fact]
    public async Task Should_Return_Unauthorized_When_Requesting_Export_Without_Authentication()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(ExportPath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_A_Versioned_File_Name_When_Requesting_Export()
    {
        var (username, password) = await CreateAdminUserAsync();
        var accessToken = await LoginAsync(username, password);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync(ExportPath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/octet-stream");

        var contentDisposition = response.Content.Headers.ContentDisposition;

        contentDisposition.ShouldNotBeNull();
        contentDisposition!.DispositionType.ShouldBe("attachment");
        contentDisposition.FileName.ShouldNotBeNullOrWhiteSpace();
        contentDisposition.FileName!
            .Trim('"')
            .ShouldMatch($@"^pot-\d{{4}}-\d{{2}}-\d{{2}}_\d{{6}}\.v{MetadataBase.CurrentVersion}\.export$");

        var content = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        content.ShouldNotBeEmpty();
    }

    private async Task<(string Username, string Password)> CreateAdminUserAsync()
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();
        var uniqueValue = Guid.NewGuid().ToString("N");
        var site = EntityFactory.CreateSite(name: $"Export Site {uniqueValue}");
        var username = $"export-{uniqueValue}";
        const string password = "Password123!";

        // The Admin role (and its permission set) is seeded by the AddRolesAndPermissions migration, so it
        // must be attached rather than added - adding it through the user graph would insert a duplicate role.
        var adminRole = await dbContext.Set<RoleEntity>()
            .AsNoTracking()
            .SingleAsync(role => role.Name == Role.Admin, TestContext.Current.CancellationToken);

        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", "Export User");

        user.PasswordHash = passwordHasher.GetHash(user, password);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.Attach(adminRole);
        user.Roles.Add(adminRole);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (username, password);
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, LoginPath)
        {
            Content = JsonContent.Create(new { Username = username, Password = password })
        };

        request.Headers.Add("User-Agent", "POT Export Test Agent/1.0");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.Status.ShouldBe(LoginSuccessStatus);
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();

        return body.AccessToken!;
    }
}
