using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Approvals;

/// <summary>
/// Proves a decision that carries an entity tag the store no longer holds is refused as a stale conflict.
/// </summary>
public class UpdateStatusFixture : IntegrationAuthFixtureBase
{
    private const string StaleEtagMessage = "a stale entity tag must not be accepted";
    private const string StaleEtagError = "The entity tag for the User does not match the current record";

    [Fact]
    public async Task Should_Return_Conflict_When_The_Etag_Is_Stale()
    {
        var platformAdmin = await CreateAdminUserAsync("approval-status", "Approval Status User");

        using var seededClient = await CreateAuthenticatedClientAsync(platformAdmin);
        var currentUser = await seededClient.GetCurrentUserAsync();

        // A user awaiting approval, which is what the decision surface lists.
        var awaitingApproval = await CreateUserInSiteAsync(currentUser.Site.RowId, "approval-status-pending",
            "Approval Status Pending", status: UserStatus.Approval);

        var tokens = await LoginAsync(platformAdmin);

        using var factory = new PlatformAdminApiWebApplicationFactory(DbHost, DbPort, platformAdmin.UserRowId);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var approvals = await client.GetPendingApprovalsAsync();
        var approval = approvals.Single(item => item.RowId == awaitingApproval.UserRowId);

        var request = new
        {
            Etag = approval.Etag - 1,
            Status = ApprovalStatus.Approved.Name
        };

        using var response = await client.PutAsJsonAsync($"/api/approvals/{approval.RowId}/status", request,
            TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);

        problem.Detail.ShouldBe(StaleEtagError);
    }
}
