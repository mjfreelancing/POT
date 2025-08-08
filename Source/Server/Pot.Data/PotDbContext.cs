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
        public DbSet<IncomeEntity> Incomes { get; set; }

        public PotDbContext(IConfiguration configuration)
        {
            _configuration = configuration.WhenNotNull();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Effectively the same as configuration["DATABASE__HOST"] when an environment variable is set (and same for others).
            var databaseHost = GetConfigurationValue("Database:Host");
            var databaseUsername = GetConfigurationValue("Database:Username");
            var databasePassword = GetConfigurationValue("Database:Password");

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
