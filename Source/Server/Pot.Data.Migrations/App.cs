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
using System.Data.Common;

namespace Pot.Data.Migrations;

internal sealed class App : ConsoleAppBase
{
    private const string DatabaseDoesNotExist = "3D000"; // invalid_catalog_name

    // A directory containing 'Source/Server/<SolutionFileName>' is the repository root, whatever the working directory is.
    private const string SolutionFileName = "pot.sln";
    private const string ErdSectionName = "Erd";
    private const string ErdOutputPathKey = "OutputPath";

    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    private readonly IDatabaseMigrator _databaseMigrator;
    private readonly IHostEnvironment _environment;
    private readonly ErdExporter _erdExporter;
    private readonly ILogger _logger;

    public App(IDatabaseMigrator databaseMigrator, IOptions<DatabaseConfiguration> databaseConfiguration,
        IHostEnvironment environment, ErdExporter erdExporter, IConfiguration configuration, ILogger<App> logger)
    {
        _ = databaseConfiguration.WhenNotNull();

        _databaseMigrator = databaseMigrator.WhenNotNull();
        _environment = environment.WhenNotNull();
        _erdExporter = erdExporter;
        _configuration = configuration.WhenNotNull();
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
                if (TryResolveErdOutputPath(out var erdOutputPath))
                {
                    _logger.LogInformation("Generating ERD at {ErdOutputPath}...", erdOutputPath);

                    await _erdExporter.ExportSchemaAsDiagramAsync(erdOutputPath);
                }
            }

            ExitCode = 0;
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);

            ExitCode = -1;
        }
        finally
        {
            _databaseMigrator.OnNewMigration -= OnNewMigration;

            _logger.LogInformation("Migration END");
        }
    }

    // Resolves the configured ERD path against the repository root, so the output location does not depend on the
    // working directory or on how many folders deep the built assembly sits.
    private bool TryResolveErdOutputPath(out string erdOutputPath)
    {
        erdOutputPath = string.Empty;

        var configuredPath = _configuration[$"{ErdSectionName}:{ErdOutputPathKey}"];

        // string.IsNullOrEmpty rather than the AllOverIt extension: its annotation is what tells the compiler the value
        // is non-null below, and nullable warnings are errors in this solution.
        if (string.IsNullOrEmpty(configuredPath))
        {
            _logger.LogWarning("ERD generation skipped: '{Section}:{Key}' is not configured.", ErdSectionName, ErdOutputPathKey);

            return false;
        }

        var repositoryRoot = FindRepositoryRoot();

        if (repositoryRoot is null)
        {
            _logger.LogWarning("ERD generation skipped: no directory containing '{SolutionPath}' was found above the application directory.", Path.Combine("Source", "Server", SolutionFileName));

            return false;
        }

        erdOutputPath = Path.Combine(repositoryRoot, configuredPath);

        return true;
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Source", "Server", SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private async Task WaitForDatabaseIsReadyAsync(CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<DbException>(dbException => dbException.SqlState != DatabaseDoesNotExist)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {retryCount} for database connection. Waiting {WaitSeconds} seconds...", retryCount, timeSpan.TotalSeconds);
                });

        try
        {
            // Use the retry policy to ensure the database is ready
            await retryPolicy.ExecuteAsync(async token =>
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(token);

                _logger.LogInformation("Database is ready.");
            }, cancellationToken);
        }
        catch (DbException exception) when (exception.SqlState == DatabaseDoesNotExist)
        {
            _logger.LogInformation("Database connection failed with '3D000' (invalid_catalog_name). Continuing execution.");
        }
    }

    private void OnNewMigration(object? sender, MigrationEventArgs eventArgs)
    {
        _logger.LogInformation("Applying migration {Migration}", eventArgs.Migration);
    }
}
