using Pot.AspNetCore.Integration.Tests.Host;

namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// Base for fixtures whose tests provision the tenants they act in, rather than seeding rows.
/// </summary>
/// <remarks>
/// Deriving from this base is what raises the host's rate limits out of the way of provisioning. A fixture that
/// does not provision keeps deriving from <see cref="IntegrationAuthFixtureBase" /> and keeps the production limits.
/// </remarks>
public abstract class ScenarioFixtureBase : IntegrationAuthFixtureBase
{
    /// <summary>
    /// Returns a client signed in as a person a scenario provisioned.
    /// </summary>
    /// <param name="user">The provisioned person, whose credentials provisioning recovered.</param>
    /// <param name="userAgent">The <c>User-Agent</c> the login is recorded against.</param>
    /// <returns>A client whose default authorization header is the issued bearer token.</returns>
    protected async Task<HttpClient> CreateScenarioClientAsync(ScenarioUser user, string userAgent = DefaultUserAgent)
    {
        var seeded = new SeededUser(user.UserRowId, user.Username, user.Password);

        return await CreateAuthenticatedClientAsync(seeded, userAgent);
    }

    /// <inheritdoc />
    protected override ApiWebApplicationFactory CreateApiWebApplicationFactory(string dbHost, int dbPort)
    {
        return new ScenarioApiWebApplicationFactory(dbHost, dbPort);
    }
}
