using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// A person a scenario invites into its tenant, with the role they hold and the state they should end up in.
/// </summary>
/// <param name="Name">A label used to refer to this person in the scenario and in failure messages.</param>
/// <param name="Role">The role the invitation grants.</param>
/// <param name="Disabled">Whether the person is disabled after they have activated their invitation.</param>
public sealed record ScenarioMember(string Name, Role Role, bool Disabled = false);
