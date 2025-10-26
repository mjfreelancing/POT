namespace Pot.App.Features.Me.Models;

public sealed class Output
{
    public sealed class SiteModel
    {
        public required Guid RowId { get; init; }
        public required long Etag { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
    }

    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public required string Email { get; init; }

    public required SiteModel Site { get; init; }
}
