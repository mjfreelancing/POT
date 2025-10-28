namespace Pot.Data.Repositories.Users.Dtos;

public sealed class GetAllUserInfo
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public required string Email { get; init; }
    public required string Status { get; init; }
    public required string[] Roles { get; init; }
    public DateTime? LastLoggedInUtc { get; init; }
}