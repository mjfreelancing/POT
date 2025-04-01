using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data
{
    public sealed class PotDbContext : DbContextBase
    {
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // TODO: Get username / password from environment variables via IConfiguration
            var databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var connectionString = $"Host={databaseHost};Database=Pot;Username=postgres;Password=password";

            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.SetPostgresVersion(new Version(13, 6));
            });
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
