using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// A state the product cannot produce, written directly instead, together with the reason it has to be.
/// </summary>
/// <param name="Reason">Why no route can reach this state. Required, and checked before anything is provisioned.</param>
/// <param name="Status">The status to write for the tenant's owner.</param>
public sealed record ScenarioDirectWrite(string Reason, UserStatus Status);
