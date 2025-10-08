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

        return new BackupSettings
        {
            Enabled = ValueConversionHelper.GetBool(keyedSettings, nameof(BackupSettings.Enabled), false),

            // Comes from the environment variable DATABASE:BACKUPPATH - must match the path used by the docker container
            Path = _databaseConfiguration.BackupPath,

            RetentionDays = ValueConversionHelper.GetInt(keyedSettings, nameof(BackupSettings.RetentionDays), 7),
            Schedule = ValueConversionHelper.GetString(keyedSettings, nameof(BackupSettings.Schedule), "0 */4 * * *")! // Default: every 4 hours
        };
    }
}
