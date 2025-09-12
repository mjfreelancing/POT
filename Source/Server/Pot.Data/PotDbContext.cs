using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;

namespace Pot.Data
{
    public sealed class PotDbContext : DbContextBase
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<IncomeEntity> Incomes { get; set; }

        public PotDbContext(DbContextOptions<PotDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sets up the many-to-many UserRole join table without an explicit model
            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.Roles)
                .WithMany(role => role.Users)
                .UsingEntity("UserRole");

            // Sets up the many-to-many RolePermission join table without an explicit model
            modelBuilder.Entity<RoleEntity>()
                .HasMany(role => role.Permissions)
                .WithMany(permission => permission.Roles)
                .UsingEntity("RolePermission");
        }
    }
}
