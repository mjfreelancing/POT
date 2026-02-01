namespace Pot.RazorComponents.Models;

// Sent to a platform administrator to approve a new user signup
public sealed class EmailPendingApprovalInfo : EmailConfigBase
{
    // The base class properties will be the platform admin details - they are receiving the email
    [EmailFormat(EmailFormatType.Both)]
    public required string UserUsername { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string UserEmail { get; init; }
}
