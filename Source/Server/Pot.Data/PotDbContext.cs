using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data
{
    public sealed class PotDbContext : DbContextBase
    {
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<IncomeEntity> Incomes { get; set; }

        public PotDbContext(DbContextOptions<PotDbContext> options)
            : base(options)
        {
        }
    }
}
