using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.EntityFrameworkCore.Migrator.Events;
using AllOverIt.GenericHost;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;
using Pot.Data.Configuration;
using Pot.Data.Extensions;

namespace Pot.Data.Migrations;

internal sealed class App : ConsoleAppBase
{
    private readonly string _connectionString;

    private readonly IDatabaseMigrator _databaseMigrator;
    private readonly IHostEnvironment _environment;
    private readonly ErdExporter _erdExporter;
    private readonly ILogger _logger;

    public App(IDatabaseMigrator databaseMigrator, IOptions<DatabaseConfiguration> databaseConfiguration,
        IHostEnvironment environment, ErdExporter erdExporter, ILogger<App> logger)
    {
        _ = databaseConfiguration.WhenNotNull();

        _databaseMigrator = databaseMigrator.WhenNotNull();
        _environment = environment.WhenNotNull();
        _erdExporter = erdExporter;
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

            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Generating ERD...");

                await _erdExporter.ExportSchemaAsDiagramAsync("..\\..\\..\\..\\..\\..\\Docs\\pot_erd.d2");
            }
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
                    _logger.LogWarning("Retry {retryCount} for database connection. Waiting {WaitSeconds} seconds...", retryCount, timeSpan.TotalSeconds);
                });

        // Use the retry policy to ensure the database is ready
        await retryPolicy.ExecuteAsync(async token =>
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(token);

            _logger.LogInformation("Database is ready.");
        }, cancellationToken);
    }

    private void OnNewMigration(object? sender, MigrationEventArgs eventArgs)
    {
        _logger.LogInformation("Applying migration {Migration}", eventArgs.Migration);
    }
}
