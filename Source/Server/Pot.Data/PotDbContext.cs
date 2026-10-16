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
    public DbSet<AuthSessionEntity> AuthSessions { get; set; }

    public PotDbContext(DbContextOptions<PotDbContext> options, ICurrentUserContext currentUserDataContext)
        : base(options)
    {
        _currentUserContext = currentUserDataContext.WhenNotNull();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Site-specific filter for Settings (a setting is always owned by a site)
        modelBuilder
            .Entity<SettingEntity>()
            .HasQueryFilter(setting => setting.Site.Id == GetCurrentUserSiteId());
    }

    /// <summary>
    /// The current user's site, resolved once per context.
    /// </summary>
    /// <remarks>
    /// Shared by the entity query filters and by the site-scoped repository reads, so the two cannot disagree
    /// about which site the caller belongs to, and the site costs one query per request rather than one per call.
    /// </remarks>
    /// <returns>The current user's site identifier.</returns>
    internal int GetCurrentUserSiteId()
    {
        _currentUserSiteId ??= Set<UserEntity>()
            .Include(user => user.Site)
            .Single(user => user.RowId == _currentUserContext.UserRowId)
            .Site.Id;

        return _currentUserSiteId.Value;
    }
}
