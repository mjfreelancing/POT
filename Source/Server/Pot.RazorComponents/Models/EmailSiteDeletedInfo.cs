namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent to a site's enabled administrators when the site is deleted.
/// </summary>
public sealed class EmailSiteDeletedInfo : EmailConfigBase
{
    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <summary>Gets the name of the deleted site.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DeletedSiteName { get; init; }

    /// <summary>Gets the display name of the administrator who deleted the site.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedByDisplayName { get; init; }

    /// <summary>Gets the username of the administrator who deleted the site.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedByUsername { get; init; }

    /// <summary>Gets the local date and time of the deletion.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required DateTime DeletedAt { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;

    /// <summary>Gets the administrator who deleted the site as "Display Name (username)".</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string DeletedByLabel => FormatUserLabel(DeletedByDisplayName, DeletedByUsername);

    /// <summary>Gets the formatted local date and time of the deletion.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string DeletedAtLabel => FormatDateTime(DeletedAt);
}
