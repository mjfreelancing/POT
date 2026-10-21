namespace Pot.RazorComponents.Models;

// Sent to a platform administrator to approve a new user signup
public sealed class EmailPendingApprovalInfo : EmailConfigBase
{
    /// <summary>Gets the display name of the platform administrator receiving the message.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    // The base class properties will be the platform admin details - they are receiving the email
    [EmailFormat(EmailFormatType.Both)]
    public required string UserUsername { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string UserEmail { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;
}
