using AllOverIt.Assertion;
using Microsoft.Extensions.Options;
using Pot.App.Concerns.Time;
using Pot.AspNetCore.Extensions;
using Pot.Data;

namespace Pot.AspNetCore.Features.DbBackup;

internal sealed class PostgresqlBackup : IPostgresqlBackup
{
    private readonly bool _isProduction;
    private readonly ITimeProvider _timeProvider;
    private readonly DatabaseConfiguration _databaseConfiguration;
    private readonly ILogger _logger;

    public PostgresqlBackup(IConfiguration configuration, ITimeProvider timeProvider,
        IOptions<DatabaseConfiguration> databaseConfiguration, ILogger<PostgresqlBackup> logger)
    {
        _isProduction = configuration.WhenNotNull().IsProduction();
        _timeProvider = timeProvider.WhenNotNull();
        _databaseConfiguration = databaseConfiguration.WhenNotNull().Value;
        _logger = logger.WhenNotNull();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dbHost = _databaseConfiguration.Host;
        var dbName = _databaseConfiguration.Name;
        var dbUser = _databaseConfiguration.Username;
        var dbPassword = _databaseConfiguration.Password;
        var dbBackupPath = _databaseConfiguration.BackupPath;

        var timestamp = _timeProvider.GetLocalDateTimeNow();
        var prefix = _isProduction ? "prod" : "dev";
        var fileName = $"{prefix}-pot-backup-{timestamp:yyyy-MM-dd_HHmmss}_utc_custom.backup";

        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $"-h {dbHost} -U {dbUser} -d {dbName} -Fc -f {dbBackupPath}/{fileName}",
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