namespace Pot.RazorComponents.Models;

public abstract class EmailConfigBase
{
    [EmailFormat(EmailFormatType.Both)]
    public required string Username { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string Email { get; init; }
}
