namespace Pot.RazorComponents.Models;

public abstract class EmailConfigBase
{
    public required string Username { get; init; }
    public required string Email { get; init; }
}
