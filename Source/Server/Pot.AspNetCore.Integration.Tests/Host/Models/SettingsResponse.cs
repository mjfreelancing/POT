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

/// <summary>
/// One setting in the settings response.
/// </summary>
public sealed class SettingResponse
{
    /// <summary>
    /// The identifier of the saved row, absent while the setting is only the category default.
    /// </summary>
    public Guid? RowId { get; set; }

    /// <summary>
    /// The entity tag an upsert must match, absent until the setting is saved for the site.
    /// </summary>
    public long? Etag { get; set; }

    /// <summary>
    /// The category the setting belongs to, by name.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// The setting's key within its category.
    /// </summary>
    public string? Key { get; set; }
}
