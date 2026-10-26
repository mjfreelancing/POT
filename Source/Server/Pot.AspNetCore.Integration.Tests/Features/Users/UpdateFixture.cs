using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Users;

/// <summary>
/// Proves each user update that carries an entity tag the store no longer holds is refused as a stale conflict.
/// </summary>
public class UpdateFixture : IntegrationAuthFixtureBase
{
    private const string StaleEtagMessage = "a stale entity tag must not be accepted";
    private const string StaleEtagError = "The entity tag for the User does not match the current record";

    [Fact]
    public async Task Should_Return_Conflict_When_The_Details_Carry_A_Stale_Etag()
    {
        var (client, target) = await CreateCallerWithTargetAsync("user-details-stale");

        using (client)
        {
            var request = new
            {
                Etag = target.Etag - 1,
                DisplayName = "Changed Display Name",
                Email = target.Email
            };

            using var response = await client.PutAsJsonAsync($"/api/users/{target.RowId}", request,
                TestContext.Current.CancellationToken);

            var problem = await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);

            problem.Detail.ShouldBe(StaleEtagError);
        }
    }

    [Fact]
    public async Task Should_Return_Conflict_When_The_Roles_Carry_A_Stale_Etag()
    {
        var (client, target) = await CreateCallerWithTargetAsync("user-roles-stale");

        using (client)
        {
            var request = new
            {
                Etag = target.Etag - 1,
                RoleIds = Array.Empty<Guid>()
            };

            using var response = await client.PutAsJsonAsync($"/api/users/{target.RowId}/roles", request,
                TestContext.Current.CancellationToken);

            var problem = await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);

            problem.Detail.ShouldBe(StaleEtagError);
        }
    }

    [Fact]
    public async Task Should_Return_Conflict_When_The_Status_Carries_A_Stale_Etag()
    {
        var (client, target) = await CreateCallerWithTargetAsync("user-status-stale");

        using (client)
        {
            var request = new
            {
                Etag = target.Etag - 1,
                Status = UserStatus.Disabled.Name
            };

            using var response = await client.PutAsJsonAsync($"/api/users/{target.RowId}/status", request,
                TestContext.Current.CancellationToken);

            var problem = await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);

            problem.Detail.ShouldBe(StaleEtagError);
        }
    }

    // Seeds an administrator and a second user in the same site, and reads the target as the user list returns it,
    // because that listing is where the entity tag an update must match comes from.
    private async Task<(HttpClient Client, UserResponse Target)> CreateCallerWithTargetAsync(string purpose)
    {
        var caller = await CreateAdminUserAsync(purpose, "Users Update Caller");
        var client = await CreateAuthenticatedClientAsync(caller);

        var currentUser = await client.GetCurrentUserAsync();
        var target = await CreateUserInSiteAsync(currentUser.Site.RowId, $"{purpose}-target", "Users Update Target");

        var users = await client.GetUsersAsync();
        var listedTarget = users.Single(user => user.RowId == target.UserRowId);

        return (client, listedTarget);
    }
}
