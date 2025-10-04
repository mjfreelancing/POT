using Microsoft.EntityFrameworkCore;

namespace Pot.Data.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void ConfigurePostgres(this DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.SetPostgresVersion(new Version(13, 6));
        });
    }
}