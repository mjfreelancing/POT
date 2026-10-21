namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent when an administrator invites a user, or re-sends an invitation. One email
/// address can belong to accounts in several sites, so the message names the site and who sent the invitation.
/// </summary>
public sealed class EmailInvitationInfo : EmailConfigBase
{
    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <summary>Gets the name of the site the user is invited to.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string SiteName { get; init; }

    /// <summary>Gets the display name of the administrator who sent the invitation.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string InvitedByDisplayName { get; init; }

    /// <summary>Gets the username of the administrator who sent the invitation.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string InvitedByUsername { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string TempPassword { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;

    /// <summary>Gets the administrator who sent the invitation as "Display Name (username)".</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string InvitedByLabel => FormatUserLabel(InvitedByDisplayName, InvitedByUsername);
}
