namespace Pot.Data.Repositories.Settings.Models;

public sealed class BackupSettings
{
    public bool Enabled { get; init; }
    public required int RetentionDays { get; init; }
    public required string Schedule { get; init; }
}