namespace Pot.AspNetCore.Features.DbBackup.Configuration;

public sealed class BackupConfiguration
{
    public required string BackupPath { get; init; }    // From environment variable DATABASE:BACKUPPATH
    public required string FilePrefix { get; set; }     // Based on environment (production or development)
}