using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.GenericHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pot.Data.Extensions;

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
                    .AddDatabaseConfiguration(hostContext.Configuration)
                    .AddDbContextFactory<PotDbContext>((provider, options) =>
                    {
                        var databaseConfiguration = provider.GetRequiredService<IOptions<DatabaseConfiguration>>().Value;
                        var connectionString = databaseConfiguration.GetConnectionString();

                        options.ConfigurePostgres(connectionString);
                    })
                    .AddScoped<IDatabaseMigrator, PotDbMigrator>();
            })
            .RunConsoleAsync(options => options.SuppressStatusMessages = true);
    }
}
