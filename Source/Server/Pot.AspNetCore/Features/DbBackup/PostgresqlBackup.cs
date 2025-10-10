using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Pot.App.Concerns.Time;
using Pot.AspNetCore.Features.DbBackup.Configuration;
using Pot.Data.Configuration;

namespace Pot.AspNetCore.Features.DbBackup;

internal sealed class PostgresqlBackup : IPostgresqlBackup
{
    private readonly ITimeProvider _timeProvider;
    private readonly DatabaseConfiguration _databaseConfiguration;
    private readonly BackupConfiguration _backupOptions;
    private readonly ILogger _logger;

    public PostgresqlBackup(ITimeProvider timeProvider, DatabaseConfiguration databaseConfiguration,
        BackupConfiguration backupOptions, ILogger<PostgresqlBackup> logger)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _databaseConfiguration = databaseConfiguration.WhenNotNull();
        _backupOptions = backupOptions.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var dbHost = _databaseConfiguration.Host;
        var dbName = _databaseConfiguration.Name;
        var dbUser = _databaseConfiguration.Username;
        var dbPassword = _databaseConfiguration.Password;

        var timestamp = _timeProvider.GetLocalDateTimeNow();
        var fileName = $"{_backupOptions.FilePrefix}-pot-{timestamp:yyyy-MM-dd_HHmmss}_utc.backup";

        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $"-h {dbHost} -U {dbUser} -d {dbName} -Fc -f {_backupOptions.BackupPath}/{fileName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    { "PGPASSWORD", dbPassword }
                }
            }
        };

        process.Start();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            _logger.LogError("pg_dump failed with error: {Error}", error);
        }
    }
}