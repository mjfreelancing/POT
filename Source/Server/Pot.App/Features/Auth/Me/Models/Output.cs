namespace Pot.App.Features.Auth.Me.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public required string Email { get; init; }
}
