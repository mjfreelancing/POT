using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Accruals.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Concerns.Accruals;

public class AccountAccrualDirtyMarkerFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public AccountAccrualDirtyMarker Marker { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        public FakeLogCollector LogCollector { get; }

        public TestContext(AccountAccrualDirtyMarker marker, PotDbContext dbContext, SiteEntity site, FakeLogCollector logCollector)
        {
            Marker = marker;
            DbContext = dbContext;
            Site = site;
            LogCollector = logCollector;
        }

        public AccountEntity AddAccount(string description)
        {
            var account = EntityFactory.CreateAccount(Site, description, balance: 1000.0d);

            DbContext.Accounts.Add(account);
            DbContext.SaveChanges();

            return account;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class Constructor : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public void Should_Throw_When_AccountAccrualRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<AccountAccrualDirtyMarker>>();
                _ = new AccountAccrualDirtyMarker(null!, logger);
            });

            exception.ParamName.ShouldBe("accountAccrualRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var repository = Substitute.For<IPersistableAccountAccrualRepository>();
                _ = new AccountAccrualDirtyMarker(repository, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class MarkDirtyForAccountAsync : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public async Task Should_Throw_When_Account_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await context.Marker.MarkDirtyForAccountAsync(null!, CancellationToken.None);
            });

            exception.ParamName.ShouldBe("account");
        }

        [Fact]
        public async Task Should_LogCall_When_Marking_Dirty_For_Account()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Logging Branch Account");

            await context.Marker.MarkDirtyForAccountAsync(account, CancellationToken.None);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(AccountAccrualDirtyMarker).FullName!,
                callerName: nameof(AccountAccrualDirtyMarker.MarkDirtyForAccountAsync),
                callerType: typeof(AccountAccrualDirtyMarker));
        }

        [Fact]
        public async Task Should_Add_AccountAccrual_When_Missing()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Create Branch Account");

            context.DbContext.AccountAccruals.Count().ShouldBe(0);

            await context.Marker.MarkDirtyForAccountAsync(account, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Added);
            accountAccrualEntry.Entity.AccountId.ShouldBe(account.Id);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Set_AccruedIsDirty_True_When_Existing_Row_Is_Clean()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clean Branch Account");

            context.DbContext.AccountAccruals.Add(new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 4, 1)
            });

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.MarkDirtyForAccountAsync(account, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Not_Modify_When_Existing_Row_Already_Dirty()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Dirty Branch Account");

            context.DbContext.AccountAccruals.Add(new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true,
                LastAccruedDate = new DateOnly(2026, 4, 2)
            });

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.MarkDirtyForAccountAsync(account, CancellationToken.None);

            var accountAccrual = await context.DbContext.AccountAccruals.SingleAsync();

            accountAccrual.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
        }
    }

    public class MarkDirtyForExpensesAsync : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public async Task Should_Throw_When_Expenses_Are_Null()
        {
            using var context = CreateTestContext();

            var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await context.Marker.MarkDirtyForExpensesAsync(null!, CancellationToken.None);
            });

            exception.ParamName.ShouldBe("expenses");
        }

        [Fact]
        public async Task Should_LogCall_When_Marking_Dirty_For_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Toggle Logging Account");
            var expenses = new List<ExpenseEntity>
            {
                EntityFactory.CreateExpense(account, false, "Toggle Logging Expense", 15.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1)
            };

            await context.Marker.MarkDirtyForExpensesAsync(expenses, CancellationToken.None);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(AccountAccrualDirtyMarker).FullName!,
                callerName: nameof(AccountAccrualDirtyMarker.MarkDirtyForExpensesAsync),
                callerType: typeof(AccountAccrualDirtyMarker));
        }

        [Fact]
        public async Task Should_Not_Modify_When_Expenses_Are_Empty()
        {
            using var context = CreateTestContext();

            await context.Marker.MarkDirtyForExpensesAsync([], CancellationToken.None);

            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Add_And_Modify_AccountAccruals_For_Affected_Accounts()
        {
            using var context = CreateTestContext();

            var existingCleanAccount = context.AddAccount("Toggle Existing Clean Account");
            var missingAccrualAccount = context.AddAccount("Toggle Missing Accrual Account");
            var existingDirtyAccount = context.AddAccount("Toggle Existing Dirty Account");

            context.DbContext.AccountAccruals.AddRange(
                new AccountAccrualEntity
                {
                    AccountId = existingCleanAccount.Id,
                    Account = existingCleanAccount,
                    AccruedIsDirty = false,
                    LastAccruedDate = new DateOnly(2026, 4, 10)
                },
                new AccountAccrualEntity
                {
                    AccountId = existingDirtyAccount.Id,
                    Account = existingDirtyAccount,
                    AccruedIsDirty = true,
                    LastAccruedDate = new DateOnly(2026, 4, 11)
                });

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            var expenses = new List<ExpenseEntity>
            {
                EntityFactory.CreateExpense(existingCleanAccount, false, "Toggle Existing Clean Expense", 25.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(missingAccrualAccount, false, "Toggle Missing Accrual Expense", 35.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(existingDirtyAccount, false, "Toggle Existing Dirty Expense", 45.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(existingCleanAccount, false, "Toggle Existing Clean Expense Duplicate Account", 55.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1)
            };

            await context.Marker.MarkDirtyForExpensesAsync(expenses, CancellationToken.None);

            var accountAccrualEntries = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .ToArray();

            accountAccrualEntries.Count(entry => entry.State == EntityState.Modified).ShouldBe(1);
            accountAccrualEntries.Count(entry => entry.State == EntityState.Added).ShouldBe(1);

            var modifiedEntry = accountAccrualEntries.Single(entry => entry.State == EntityState.Modified);
            modifiedEntry.Entity.AccountId.ShouldBe(existingCleanAccount.Id);
            modifiedEntry.Entity.AccruedIsDirty.ShouldBeTrue();

            var addedEntry = accountAccrualEntries.Single(entry => entry.State == EntityState.Added);
            addedEntry.Entity.AccountId.ShouldBe(missingAccrualAccount.Id);
            addedEntry.Entity.AccruedIsDirty.ShouldBeTrue();
        }
    }

    public class GetAccountIdsToMarkDirty : AccountAccrualDirtyMarkerFixture
    {
        [Fact]
        public void Should_LogCall_When_Getting_AccountIds_For_Update()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 15);
            var after = CreateDirtyState(accountId: 15);

            _ = context.Marker.GetAccountIdsToMarkDirty(before, after);

            context.LogCollector.ShouldContainLogCall(
                category: typeof(AccountAccrualDirtyMarker).FullName!,
                callerName: nameof(AccountAccrualDirtyMarker.GetAccountIdsToMarkDirty),
                callerType: typeof(AccountAccrualDirtyMarker));
        }

        [Fact]
        public void Should_Throw_When_Before_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var after = CreateDirtyState(accountId: 11);

                _ = context.Marker.GetAccountIdsToMarkDirty(null!, after);
            });

            exception.ParamName.ShouldBe("before");
        }

        [Fact]
        public void Should_Throw_When_After_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var before = CreateDirtyState(accountId: 11);

                _ = context.Marker.GetAccountIdsToMarkDirty(before, null!);
            });

            exception.ParamName.ShouldBe("after");
        }

        [Fact]
        public void Should_Return_Empty_When_Update_Is_Not_Dirty_Impacting()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 11);
            var after = CreateDirtyState(accountId: 11);

            var accountIds = context.Marker.GetAccountIdsToMarkDirty(before, after);

            accountIds.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Return_Single_Account_When_Dirty_Impact_Is_Same_Account()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 12);
            var after = CreateDirtyState(accountId: 12, mutate: state => state with { Amount = 220.0d });

            var accountIds = context.Marker.GetAccountIdsToMarkDirty(before, after);

            accountIds.ShouldBe([12]);
        }

        [Fact]
        public void Should_Return_Both_Accounts_When_Reassigned()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 13);
            var after = CreateDirtyState(accountId: 14);

            var accountIds = context.Marker.GetAccountIdsToMarkDirty(before, after);

            accountIds.ShouldBe([13, 14]);
        }

        private static ExpenseAccrualState CreateDirtyState(int accountId, Func<ExpenseAccrualState, ExpenseAccrualState>? mutate = null)
        {
            var state = new ExpenseAccrualState
            {
                AccountId = accountId,
                ExcludeFromCalcs = false,
                AccrualStart = new DateOnly(2026, 1, 1),
                NextDue = new DateOnly(2026, 2, 1),
                EndDate = null,
                AccrualPolicy = AccrualPolicy.Automatic,
                Frequency = Frequency.Months,
                FrequencyCount = 1,
                Amount = 120.0d
            };

            return mutate?.Invoke(state) ?? state;
        }
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

        var repositoryLogger = new FakeLogger<AccountAccrualRepository>();
        var markerLogCollector = new FakeLogCollector();
        var markerLogger = new FakeLogger<AccountAccrualDirtyMarker>(markerLogCollector);

        var repository = new AccountAccrualRepository(dbContext, repositoryLogger);
        var marker = new AccountAccrualDirtyMarker(repository, markerLogger);

        return new TestContext(marker, dbContext, site, markerLogCollector);
    }
}
