using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.GenericHost;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace Pot.Data.Migrations;

internal sealed class App : ConsoleAppBase
{
    private readonly IDatabaseMigrator _databaseMigrator;
    private readonly ILogger _logger;

    public App(IDatabaseMigrator databaseMigrator, ILogger<App> logger)
    {
        _databaseMigrator = databaseMigrator.WhenNotNull();
        _logger = logger.WhenNotNull();

        Console.WriteLine();
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration START");

        try
        {
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

    private void OnNewMigration(object? sender, AllOverIt.EntityFrameworkCore.Migrator.Events.MigrationEventArgs eventArgs)
    {
        _logger.LogInformation("Applying migration {Migration}", eventArgs.Migration);
    }
}
