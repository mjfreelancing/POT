using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pot.Data.Extensions;

namespace Pot.Data.Migrations;

// Only used for design time migrations
public class PotDbContextFactory : IDesignTimeDbContextFactory<PotDbContext>
{
    public PotDbContext CreateDbContext(string[] args)
    {
        var configuration = ConfigurationFactory.Create();

        var options = new DbContextOptionsBuilder<PotDbContext>();
        options.ConfigurePostgres(configuration);

        return new PotDbContext(options.Options);
    }
}
