using AllOverIt.EntityFrameworkCore.Migrator;
using AllOverIt.GenericHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    .AddDbContextFactory<PotDbContext>(options =>
                    {
                        var configuration = ConfigurationFactory.Create();
                        options.ConfigurePostgres(configuration);
                    })
                    .AddScoped<IDatabaseMigrator, PotDbMigrator>();
            })
            .RunConsoleAsync(options => options.SuppressStatusMessages = true);
    }
}
