using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pot.Data.Entities;
using Pot.Data.Repositories.Settings.Helpers;
using Pot.Data.Repositories.Settings.Models;
using Pot.Shared;

namespace Pot.Data.Repositories.Settings;

internal sealed class SettingsRepository : GenericRepository<PotDbContext, SettingEntity>, ISettingsRepository
{
    private readonly DatabaseConfiguration _databaseConfiguration;

    public SettingsRepository(PotDbContext dbContext, IOptions<DatabaseConfiguration> databaseConfiguration)
        : base(dbContext)
    {
        _databaseConfiguration = databaseConfiguration.WhenNotNull().Value;
    }

    public async Task<BackupSettings> GetDatabaseSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await Current
            .Where(setting => setting.Category == SettingCategory.Backup)
            .Select(setting => new
            {
                setting.Key,
                setting.Value
            })
            .ToListAsync(cancellationToken);

        var keyedSettings = settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Using the environment variable POSTGRES_BACKUP_PATH if present since this is what a container will be using so it can mount a volume.
        // If not running in a container, then use the value from the database settings.
        var backupPath = _databaseConfiguration.BackupPath.IsNullOrEmpty()
            ? ValueConversionHelper.GetString(keyedSettings, nameof(BackupSettings.Path), null)
            : _databaseConfiguration.BackupPath;

        return new BackupSettings
        {
            Enabled = ValueConversionHelper.GetBool(keyedSettings, nameof(BackupSettings.Enabled), false),
            Path = backupPath,
            RetentionDays = ValueConversionHelper.GetInt(keyedSettings, nameof(BackupSettings.RetentionDays), 7),
            Schedule = ValueConversionHelper.GetString(keyedSettings, nameof(BackupSettings.Schedule), "15 * * *")! // Default: 15 minutes past the hour, every hour
        };
    }
}
