namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// A person a scenario provisioned, with the credentials that let a test act as them.
/// </summary>
/// <param name="UserRowId">The user's identifier, as the product created it.</param>
/// <param name="Username">The name the person signs in with.</param>
/// <param name="Password">The password the person signs in with.</param>
public sealed record ScenarioUser(Guid UserRowId, string Username, string Password);
