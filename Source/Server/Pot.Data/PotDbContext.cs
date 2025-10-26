using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Entities;
using Pot.Shared;

namespace Pot.Data;

public sealed class PotDbContext : DbContextBase
{
    private readonly ICurrentUserContext _currentUserContext;
    private int? _currentUserSiteId;

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<ExpenseEntity> Expenses { get; set; }
    public DbSet<IncomeEntity> Incomes { get; set; }
    public DbSet<SettingEntity> Settings { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<OneTimePasswordEntity> OneTimePasswords { get; set; }

    public PotDbContext(DbContextOptions<PotDbContext> options, ICurrentUserContext currentUserDataContext)
        : base(options)
    {
        _currentUserContext = currentUserDataContext.WhenNotNull();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<ExpenseEntity>()
            .Property(expense => expense.AccruedIsDirty)
            .HasDefaultValue(true);

        // Sets up the many-to-many UserRole join table without an explicit model
        modelBuilder
            .Entity<UserEntity>()
            .HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity("UserRole");

        // Sets up the many-to-many RolePermission join table without an explicit model
        modelBuilder
            .Entity<RoleEntity>()
            .HasMany(role => role.Permissions)
            .WithMany(permission => permission.Roles)
            .UsingEntity("RolePermission");

        // Default AttemptCount to zero for OTPs
        modelBuilder
            .Entity<OneTimePasswordEntity>()
            .Property(otp => otp.AttemptCount)
            .HasDefaultValue(0);

        // Global query filter to only return entities for the current user's site
        SetupQueryFilters(modelBuilder);
    }

    private void SetupQueryFilters(ModelBuilder modelBuilder)
    {
        // Site-specific filter for Accounts
        modelBuilder
            .Entity<AccountEntity>()
            .HasQueryFilter(account => account.Site.Id == GetCurrentUserSiteId());

        // Site-specific filter for Expenses (via Account relationship)
        modelBuilder
            .Entity<ExpenseEntity>()
            .HasQueryFilter(expense => expense.Account.Site.Id == GetCurrentUserSiteId());

        // Site-specific filter for Incomes (via Account relationship)
        modelBuilder
            .Entity<IncomeEntity>()
            .HasQueryFilter(income => income.Account.Site.Id == GetCurrentUserSiteId());

        // Query Settings (if they have a direct Site relationship)
        modelBuilder
            .Entity<SettingEntity>()
            .HasQueryFilter(setting => setting.Site != null && setting.Site.Id == GetCurrentUserSiteId());
    }

    private int GetCurrentUserSiteId()
    {
        _currentUserSiteId ??= Set<UserEntity>()
            .Include(user => user.Site)
            .Single(user => user.RowId == _currentUserContext.UserRowId)
            .Site.Id;

        return _currentUserSiteId.Value;
    }
}
