namespace Pot.App.Features.Users.Invite.Models;

public sealed class Input
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required Guid[] RoleIds { get; init; }
}