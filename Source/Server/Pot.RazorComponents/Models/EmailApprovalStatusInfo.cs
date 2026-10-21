namespace Pot.RazorComponents.Models;

// Used for Approved and Rejected Approval emails
public sealed class EmailApprovalStatusInfo : EmailConfigBase
{
    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;
}
