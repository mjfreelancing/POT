using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pot.Data.Entities;

namespace Pot.Data
{
    public sealed class PotDbContext : DbContextBase
    {
        private readonly IConfiguration _configuration;

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }

        public PotDbContext(IConfiguration configuration)
        {
            _configuration = configuration.WhenNotNull();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var databaseHost = GetConfigurationValue("DATABASE_HOST");
            var databaseUsername = GetConfigurationValue("DATABASE_USERNAME");
            var databasePassword = GetConfigurationValue("DATABASE_PASSWORD");

            var connectionString = $"Host={databaseHost};Database=Pot;Username={databaseUsername};Password={databasePassword}";

            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.SetPostgresVersion(new Version(13, 6));
            });
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private string GetConfigurationValue(string key)
        {
            return _configuration[key] ?? throw new InvalidOperationException($"Configuration value for '{key}' is not set.");
        }
    }
}
