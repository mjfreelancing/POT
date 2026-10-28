namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The settings the integration fixtures assert against, as the settings endpoint returns them.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoint returns more, which deserialization ignores.
/// </remarks>
public sealed class SettingsResponse
{
    /// <summary>
    /// Every setting of every category, including the ones a site has not saved yet.
    /// </summary>
    public SettingResponse[] Settings { get; set; } = [];
}
