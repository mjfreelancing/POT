namespace Pot.RazorComponents.Models;

public sealed class EmailInvitationInfo : EmailConfigBase
{
    public required string TempPassword { get; init; }
}
