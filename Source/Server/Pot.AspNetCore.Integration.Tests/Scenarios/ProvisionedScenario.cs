namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// A tenant a scenario provisioned, with everything a test needs to act in it.
/// </summary>
/// <param name="Name">The scenario's name.</param>
/// <param name="SiteRowId">The tenant's identifier, as the product created it.</param>
/// <param name="Owner">The administrator created by signing up, whose credentials drove the provisioning.</param>
/// <param name="Members">The invited people, keyed by the label they were declared with.</param>
public sealed record ProvisionedScenario(string Name, Guid SiteRowId, ScenarioUser Owner,
    IReadOnlyDictionary<string, ScenarioUser> Members);
