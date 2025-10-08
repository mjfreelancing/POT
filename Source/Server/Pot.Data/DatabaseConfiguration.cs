namespace Pot.Data;

public sealed class DatabaseConfiguration
{
    // Environment variable: DATABASE:NAME
    public required string Name { get; init; }

    // Environment variable: DATABASE:HOST
    public required string Host { get; init; }

    // Environment variable: DATABASE:USERNAME
    public required string Username { get; init; }

    // Environment variable: DATABASE:PASSWORD
    public required string Password { get; init; }

    // Environment variable: DATABASE:BACKUPPATH
    public required string BackupPath { get; init; }
}
