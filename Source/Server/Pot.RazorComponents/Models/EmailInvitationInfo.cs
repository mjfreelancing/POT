namespace Pot.RazorComponents.Models;

public sealed class EmailInvitationInfo : EmailConfigBase
{
    [EmailFormat(EmailFormatType.Both)]
    public required string TempPassword { get; init; }
}
