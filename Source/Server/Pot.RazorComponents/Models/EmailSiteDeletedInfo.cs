namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent to a site's enabled administrators when the site is deleted.
/// </summary>
public sealed class EmailSiteDeletedInfo : EmailConfigBase
{
    /// <summary>Gets the name of the deleted site.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DeletedSiteName { get; init; }

    /// <summary>Gets the username of the administrator who deleted the site.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DeletedByUsername { get; init; }
}
