namespace Pot.AspNetCore.Integration.Tests.Scenarios;

/// <summary>
/// Raised when a scenario could not be provisioned, naming the step that failed so the failure reads as a broken
/// harness rather than as a defect in the behaviour under test.
/// </summary>
public sealed class ScenarioProvisioningException : Exception
{
    /// <summary>
    /// The provisioning step that failed.
    /// </summary>
    public string Step { get; }

    /// <summary>
    /// What went wrong at that step.
    /// </summary>
    public string Detail { get; }

    /// <summary>
    /// Creates the exception for a step that failed.
    /// </summary>
    /// <param name="step">The provisioning step that failed, phrased as the action it was attempting.</param>
    /// <param name="detail">What went wrong, including a status and body where a route answered.</param>
    public ScenarioProvisioningException(string step, string detail)
        : base($"Scenario provisioning failed at '{step}': {detail}")
    {
        Step = step;
        Detail = detail;
    }
}
