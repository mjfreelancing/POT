using Pot.AspNetCore.Integration.Tests.Scenarios;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Features.Scenarios;

/// <summary>
/// Proves a scenario that cannot be provisioned fails with the step it reached and the answer it was given, so a
/// broken harness is never mistaken for a defect in the behaviour under test.
/// </summary>
public class ScenarioProvisioningRefusalFixture : ScenarioFixtureBase
{
    [Fact]
    public async Task Should_Name_The_Step_And_The_Answer_When_Provisioning_Cannot_Finish()
    {
        var definition = new ScenarioDefinition("unapprovable");

        var exception = await Should.ThrowAsync<ScenarioProvisioningException>(async () =>
            await ProvisionScenarioAsync(definition, TestContext.Current.CancellationToken));

        exception.Step.ShouldBe("list the pending approvals");
        exception.Detail.ShouldContain("403");
        exception.Message.ShouldContain("list the pending approvals");
    }

    /// <summary>
    /// Signs in an administrator the host does not treat as a platform administrator, so the approval the
    /// provisioning needs is refused at the first route that requires it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A client that is authenticated but not authorized to decide approvals.</returns>
    protected override async Task<HttpClient> CreatePlatformApproverClientAsync(CancellationToken cancellationToken)
    {
        var administrator = await CreateAdminUserAsync("plain-approver", "Plain Approver");

        return await CreateAuthenticatedClientAsync(administrator);
    }
}
