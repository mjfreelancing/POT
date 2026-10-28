namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// A tenant declared as data: its name, the people it holds and any state that has to be written directly.
/// </summary>
/// <param name="Name">The scenario's name. Every identifier provisioned from it is derived from this and a run-unique suffix.</param>
/// <param name="Members">The people invited into the tenant, besides the owner created by signing up.</param>
/// <param name="DirectWrite">A state to write directly after provisioning, when one is needed.</param>
public sealed record ScenarioDefinition(string Name, IReadOnlyList<ScenarioMember> Members, ScenarioDirectWrite? DirectWrite = null)
{
    /// <summary>
    /// Declares a scenario whose tenant holds only the administrator created by signing up.
    /// </summary>
    /// <param name="name">The scenario's name.</param>
    public ScenarioDefinition(string name)
        : this(name, [])
    {
    }
}
