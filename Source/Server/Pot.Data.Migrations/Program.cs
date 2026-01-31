using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.GenericHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pot.Data.Configuration;
using Pot.Data.Extensions;
using Pot.Shared;

namespace Pot.Data.Migrations;

internal class Program
{
    static async Task Main(string[] args)
    {
        await GenericHost
            .CreateConsoleHostBuilder<App>(args)
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<ICurrentUserContext, NullCurrentUserContext>()
                    .AddDatabaseConfiguration()
                    .AddDbContextFactory<PotDbContext>((provider, options) =>
                    {
                        var databaseConfiguration = provider.GetRequiredService<DatabaseConfiguration>();
                        var connectionString = databaseConfiguration.GetConnectionString();

                        options.ConfigurePostgres(connectionString);
                    })
                    .AddSingleton<ErdExporter>()
                    .AddSingleton<IDatabaseMigrator, PotDbMigrator>();
            })
            .RunConsoleAsync(options => options.SuppressStatusMessages = true);
    }
}
