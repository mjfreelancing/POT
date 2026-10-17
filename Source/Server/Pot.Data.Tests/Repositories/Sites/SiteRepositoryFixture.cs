using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Users;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Sites;

public class SiteRepositoryFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public SiteRepository Repository { get; }
        public SiteEntity Site { get; }
        public UserEntity User { get; }

        public TestContext(PotDbContext dbContext, SiteRepository repository, SiteEntity site, UserEntity user)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            User = user;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetCurrentSite : SiteRepositoryFixture
    {
        [Fact]
        public void Should_Return_The_Site_Of_The_Current_User()
        {
            using var context = CreateTestContext();

            var result = context.Repository.GetCurrentSite();

            result.RowId.ShouldBe(context.Site.RowId);
            result.Name.ShouldBe(context.Site.Name);
        }
    }

    public class DeleteCurrentSiteWithDependentsAsync : SiteRepositoryFixture
    {
        [Fact]
        public async Task Should_Remove_Every_Record_Belonging_To_The_Current_Site()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            var callerSession = CreateSession(context.Caller);
            var colleagueSession = CreateSession(context.Colleague);
            var linkedOtp = CreateOtp(context.Colleague.Username, OtpReason.PasswordReset);
            var signupOtp = CreateOtp(context.Caller.Username, OtpReason.Signup);

            await context.SeedAsync(callerSession, colleagueSession, signupOtp);
            await context.SeedOtpAsync(linkedOtp, context.Colleague);

            var counts = await context.Repository.DeleteCurrentSiteWithDependentsAsync(cancellationToken);

            counts.Expenses.ShouldBe(1);
            counts.Incomes.ShouldBe(1);
            counts.Accounts.ShouldBe(1);
            counts.Settings.ShouldBe(1);
            counts.OneTimePasswords.ShouldBe(2);
            counts.Sessions.ShouldBe(2);
            counts.Users.ShouldBe(2);

            var siteRemains = await context.DbContext.Set<SiteEntity>().AnyAsync(site => site.RowId == context.Site.RowId, cancellationToken);
            var usersRemain = await context.DbContext.Users.AnyAsync(user => user.Site.Id == context.Site.Id, cancellationToken);
            var accountsRemain = await context.DbContext.Accounts.IgnoreQueryFilters().AnyAsync(account => account.Site.Id == context.Site.Id, cancellationToken);
            var expensesRemain = await context.DbContext.Expenses.IgnoreQueryFilters().AnyAsync(expense => expense.Account.Site.Id == context.Site.Id, cancellationToken);
            var incomesRemain = await context.DbContext.Incomes.IgnoreQueryFilters().AnyAsync(income => income.Account.Site.Id == context.Site.Id, cancellationToken);
            var settingsRemain = await context.DbContext.Settings.IgnoreQueryFilters().AnyAsync(setting => setting.Site.Id == context.Site.Id, cancellationToken);
            var sessionsRemain = await context.DbContext.AuthSessions.AnyAsync(session => session.UserId == context.Caller.Id, cancellationToken);
            var otpsRemain = await context.DbContext.OneTimePasswords.AnyAsync(otp => otp.Username == context.Caller.Username, cancellationToken);

            siteRemains.ShouldBeFalse();
            usersRemain.ShouldBeFalse();
            accountsRemain.ShouldBeFalse();
            expensesRemain.ShouldBeFalse();
            incomesRemain.ShouldBeFalse();
            settingsRemain.ShouldBeFalse();
            sessionsRemain.ShouldBeFalse();
            otpsRemain.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Leave_Another_Site_Untouched()
        {
            var cancellationToken = Xunit.TestContext.Current.CancellationToken;

            using var context = CreateDeletionTestContext();

            await context.Repository.DeleteCurrentSiteWithDependentsAsync(cancellationToken);

            var siteRemains = await context.DbContext.Set<SiteEntity>().AnyAsync(site => site.RowId == context.OtherSite.RowId, cancellationToken);
            var usersRemain = await context.DbContext.Users.AnyAsync(user => user.RowId == context.OtherUser.RowId, cancellationToken);
            var accountsRemain = await context.DbContext.Accounts.IgnoreQueryFilters().AnyAsync(account => account.Site.Id == context.OtherSite.Id, cancellationToken);
            var expensesRemain = await context.DbContext.Expenses.IgnoreQueryFilters().AnyAsync(expense => expense.Account.Site.Id == context.OtherSite.Id, cancellationToken);
            var incomesRemain = await context.DbContext.Incomes.IgnoreQueryFilters().AnyAsync(income => income.Account.Site.Id == context.OtherSite.Id, cancellationToken);
            var settingsRemain = await context.DbContext.Settings.IgnoreQueryFilters().AnyAsync(setting => setting.Site.Id == context.OtherSite.Id, cancellationToken);

            siteRemains.ShouldBeTrue();
            usersRemain.ShouldBeTrue();
            accountsRemain.ShouldBeTrue();
            expensesRemain.ShouldBeTrue();
            incomesRemain.ShouldBeTrue();
            settingsRemain.ShouldBeTrue();
        }
    }

    private sealed class DeletionTestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public SiteRepository Repository { get; }
        public SiteEntity Site { get; }
        public SiteEntity OtherSite { get; }
        public UserEntity Caller { get; }
        public UserEntity Colleague { get; }
        public UserEntity OtherUser { get; }

        public DeletionTestContext(PotDbContext dbContext, SiteRepository repository, SiteEntity site, SiteEntity otherSite,
            UserEntity caller, UserEntity colleague, UserEntity otherUser)
        {
            DbContext = dbContext;
            Repository = repository;
            Site = site;
            OtherSite = otherSite;
            Caller = caller;
            Colleague = colleague;
            OtherUser = otherUser;
        }

        public Task<int> SeedAsync(params EntityBase[] entities)
        {
            DbContext.AddRange(entities);

            return SaveAndDetachAsync();
        }

        public Task<int> SeedOtpAsync(OneTimePasswordEntity otp, UserEntity user)
        {
            DbContext.Add(otp);

            // The one-time password has no user property, only the relationship, so the link is set through
            // the shadow foreign key after the row is tracked.
            DbContext.Entry(otp).Property<int?>("UserId").CurrentValue = user.Id;

            return SaveAndDetachAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }

        private async Task<int> SaveAndDetachAsync()
        {
            var result = await DbContext.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            DbContext.ChangeTracker.Clear();

            return result;
        }
    }

    private static DeletionTestContext CreateDeletionTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite(name: $"Site {Guid.NewGuid():N}");
        var otherSite = EntityFactory.CreateSite(name: $"Other Site {Guid.NewGuid():N}");
        var caller = EntityFactory.CreateUser(site, $"caller-{Guid.NewGuid():N}");
        var colleague = EntityFactory.CreateUser(site, $"colleague-{Guid.NewGuid():N}");
        var otherUser = EntityFactory.CreateUser(otherSite, $"other-{Guid.NewGuid():N}");

        var account = EntityFactory.CreateAccount(site, $"Main {Guid.NewGuid():N}", 100);
        var expense = EntityFactory.CreateExpense(account, excludeFromCalc: false, "Expense", 10, "2026-01-01", "2026-02-01", null, Frequency.Months, 1);
        var income = EntityFactory.CreateIncome(account, excludeFromCalc: false, "Income", 10, "2026-01-01", null, Frequency.Months, 1);
        var setting = new SettingEntity
        {
            RowId = Guid.NewGuid(),
            Category = SettingCategory.EmailBudgetReminder,
            Key = "theme",
            Value = "dark",
            Site = site
        };

        var otherAccount = EntityFactory.CreateAccount(otherSite, $"Other {Guid.NewGuid():N}", 50);
        var otherExpense = EntityFactory.CreateExpense(otherAccount, excludeFromCalc: false, "Other Expense", 10, "2026-01-01", "2026-02-01", null, Frequency.Months, 1);
        var otherIncome = EntityFactory.CreateIncome(otherAccount, excludeFromCalc: false, "Other Income", 10, "2026-01-01", null, Frequency.Months, 1);
        var otherSetting = new SettingEntity
        {
            RowId = Guid.NewGuid(),
            Category = SettingCategory.EmailBudgetReminder,
            Key = "theme",
            Value = "light",
            Site = otherSite
        };

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(caller.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.AddRange(site, otherSite, caller, colleague, otherUser, account, expense, income, setting,
            otherAccount, otherExpense, otherIncome, otherSetting);

        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var userRepository = new UserRepository(dbContext, currentUserContext, Substitute.For<ILogger<UserRepository>>());
        var repository = new SiteRepository(dbContext, userRepository);

        return new DeletionTestContext(dbContext, repository, site, otherSite, caller, colleague, otherUser);
    }

    private static AuthSessionEntity CreateSession(UserEntity user)
    {
        return new AuthSessionEntity
        {
            RowId = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };
    }

    private static OneTimePasswordEntity CreateOtp(string username, OtpReason reason)
    {
        return new OneTimePasswordEntity
        {
            RowId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Username = username,
            Email = $"{username}@example.com",
            Reason = reason,
            RefCode = "123456",
            OtpCode = "654321",
            Status = OtpStatus.Active,
            CreatedUtc = DateTime.UtcNow,
            ExpiryUtc = DateTime.UtcNow.AddMinutes(10)
        };
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.SaveChanges();

        var userRepository = new UserRepository(dbContext, currentUserContext, Substitute.For<ILogger<UserRepository>>());
        var repository = new SiteRepository(dbContext, userRepository);

        return new TestContext(dbContext, repository, site, user);
    }
}
