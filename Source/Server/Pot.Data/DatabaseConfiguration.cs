namespace Pot.Data;

public sealed class DatabaseConfiguration
{
    // Environment variable: POSTGRES_DB
    public required string Name { get; init; }

    // Environment variable: POSTGRES_HOST
    public required string Host { get; init; }

    // Environment variable: POSTGRES_USER
    public required string Username { get; init; }

    // Environment variable: POSTGRES_PASSWORD
    public required string Password { get; init; }

    // Environment variable: POSTGRES_BACKUP_PATH
    public required string BackupPath { get; init; }
}
