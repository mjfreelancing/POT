using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;

namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// The host a scenario is provisioned against: somewhere to make requests, somewhere to read what the routes
/// wrote, the messages the application sent, and a way to act as a platform administrator.
/// </summary>
public interface IScenarioHost
{
    /// <summary>
    /// The messages the application has submitted.
    /// </summary>
    CapturingEmailWriter Emails { get; }

    /// <summary>
    /// Creates a client that carries no credentials, for the steps that happen before an account exists.
    /// </summary>
    /// <returns>A client for the fixture's host.</returns>
    HttpClient CreateAnonymousClient();

    /// <summary>
    /// Creates a scope for reading what the routes wrote.
    /// </summary>
    /// <returns>A scope to resolve the data context from.</returns>
    IServiceScope CreateDataScope();

    /// <summary>
    /// Creates a client already signed in as a platform administrator, for the approval surface.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A client whose bearer token belongs to a platform administrator.</returns>
    Task<HttpClient> CreatePlatformApproverClientAsync(CancellationToken cancellationToken);
}
