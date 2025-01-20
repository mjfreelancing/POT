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
            optionsBuilder.UseNpgsql("Host=localhost;Database=Pot;Username=postgres;Password=password", options =>
            {
                options.SetPostgresVersion(new Version(13, 6));
            });
        }
    }
}
