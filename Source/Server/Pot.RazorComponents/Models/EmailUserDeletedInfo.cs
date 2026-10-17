namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent when a user is deleted. It goes to the deleted user and to the site's
/// enabled administrators, so the wording names the deleted user rather than addressing them directly.
/// </summary>
public sealed class EmailUserDeletedInfo : EmailConfigBase
{
    /// <summary>Gets the username of the deleted user.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DeletedUsername { get; init; }

    /// <summary>Gets who deleted the user: an administrator's username, or "themselves" for self-deletion.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DeletedByLabel { get; init; }

    /// <summary>Gets the name of the site the user belonged to.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string SiteName { get; init; }
}
