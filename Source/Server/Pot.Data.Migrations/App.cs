using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.GenericHost;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;
using Pot.Data.Configuration;
using Pot.Data.Extensions;

namespace Pot.Data.Migrations;

internal sealed class App : ConsoleAppBase
{
    private readonly IDatabaseMigrator _databaseMigrator;
    private readonly ILogger _logger;
    private readonly string _connectionString;

    public App(IDatabaseMigrator databaseMigrator, IOptions<DatabaseConfiguration> databaseConfiguration, ILogger<App> logger)
    {
        _databaseMigrator = databaseMigrator.WhenNotNull();
        _ = databaseConfiguration.WhenNotNull();
        _logger = logger.WhenNotNull();

        _connectionString = databaseConfiguration.Value.GetConnectionString();
    }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration START");

        try
        {
            await WaitForDatabaseIsReadyAsync(cancellationToken);

            _databaseMigrator.OnNewMigration += OnNewMigration;

            _logger.LogInformation("Checking for new migrations");

            await _databaseMigrator.MigrateAsync();
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }
        finally
        {
            _databaseMigrator.OnNewMigration -= OnNewMigration;

            _logger.LogInformation("Migration END");
        }

        ExitCode = 0;
    }

    private async Task WaitForDatabaseIsReadyAsync(CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {retryCount} for database connection. Waiting {waitSeconds} seconds...", retryCount, timeSpan.TotalSeconds);
                });

        // Use the retry policy to ensure the database is ready
        await retryPolicy.ExecuteAsync(async token =>
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(token);

            _logger.LogInformation("Database is ready.");
        }, cancellationToken);
    }

    private void OnNewMigration(object? sender, AllOverIt.EntityFrameworkCore.Migrator.Events.MigrationEventArgs eventArgs)
    {
        _logger.LogInformation("Applying migration {Migration}", eventArgs.Migration);
    }
}
