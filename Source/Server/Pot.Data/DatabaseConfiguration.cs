namespace Pot.Data;

public sealed class DatabaseConfiguration
{
    public required string Name { get; init; }
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
