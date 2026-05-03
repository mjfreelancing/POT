using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Accruals.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Concerns.Accruals;

public class AccrualDirtyStateManagerFixture : PotFixtureBase
{
    private sealed class TestContext : IDisposable
    {
        public AccrualDirtyStateManager Marker { get; }
        public PotDbContext DbContext { get; }
        public SiteEntity Site { get; }
        private ILogger<AccrualDirtyStateManager> Logger { get; }

        public TestContext(AccrualDirtyStateManager marker, PotDbContext dbContext, SiteEntity site, ILogger<AccrualDirtyStateManager> logger)
        {
            Marker = marker;
            DbContext = dbContext;
            Site = site;
            Logger = logger;
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        public LoggerCallContext CaptureLogCalls(Action action)
        {
            return Logger.CaptureLogCalls(action);
        }

        public Task<LoggerCallContext> CaptureLogCallsAsync(Func<Task> action)
        {
            return Logger.CaptureLogCallsAsync(action);
        }
        */

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

    public class Constructor : AccrualDirtyStateManagerFixture
    {
        private readonly IPersistableAccountAccrualRepository _accountAccrualRepositoryFake;
        private readonly ILogger<AccrualDirtyStateManager> _loggerFake;

        public Constructor()
        {
            _accountAccrualRepositoryFake = Substitute.For<IPersistableAccountAccrualRepository>();
            _loggerFake = Substitute.For<ILogger<AccrualDirtyStateManager>>();
        }

        [Fact]
        public void Should_Throw_When_AccountAccrualRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new AccrualDirtyStateManager(null!, _loggerFake);
            });

            exception.ParamName.ShouldBe("accountAccrualRepository");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new AccrualDirtyStateManager(_accountAccrualRepositoryFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class GetAccountsRequiringRecalc : AccrualDirtyStateManagerFixture
    {
        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public void Should_LogCall_When_Getting_AccountIds_For_Update()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 15);
            var after = CreateDirtyState(accountId: 15);
            var asOfDate = new DateOnly(2026, 2, 1);

            var logContext = context.CaptureLogCalls(() =>
            {
                _ = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);
            });

            _ = logContext.ShouldLogCall<AccrualDirtyStateManager>(nameof(AccrualDirtyStateManager.GetAccountsRequiringRecalc));
        }
        */

        [Fact]
        public void Should_Throw_When_Before_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var after = CreateDirtyState(accountId: 11);

                _ = context.Marker.GetAccountsRequiringRecalc(null!, after, new DateOnly(2026, 2, 1));
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

                _ = context.Marker.GetAccountsRequiringRecalc(before, null!, new DateOnly(2026, 2, 1));
            });

            exception.ParamName.ShouldBe("after");
        }

        [Fact]
        public void Should_Return_Empty_When_Update_Is_Not_Dirty_Impacting()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 11);
            var after = CreateDirtyState(accountId: 11);

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, new DateOnly(2026, 2, 1));

            accountIds.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Return_Empty_When_Expense_Remains_Excluded_After_Edit()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 16, mutate: state => state with { ExcludeFromCalcs = true });
            var after = CreateDirtyState(accountId: 16, mutate: state => state with { ExcludeFromCalcs = true, Amount = 220.0d });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, new DateOnly(2026, 2, 1));

            accountIds.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Return_Single_Account_When_Dirty_Impact_Is_Same_Account()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 12);
            var after = CreateDirtyState(accountId: 12, mutate: state => state with { Amount = 220.0d });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, new DateOnly(2026, 2, 1));

            accountIds.ShouldBe([12]);
        }

        [Fact]
        public void Should_Return_Both_Accounts_When_Reassigned()
        {
            using var context = CreateTestContext();

            var before = CreateDirtyState(accountId: 13);
            var after = CreateDirtyState(accountId: 14);

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, new DateOnly(2026, 2, 1));

            accountIds.ShouldBe([13, 14]);
        }

        [Fact]
        public void Should_Return_Empty_When_Both_States_Are_Ended_OneTime()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 17, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 8)
            });

            var after = CreateDirtyState(accountId: 17, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 220.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Return_Single_Account_When_Both_States_Are_OneTime_And_EndDate_Equals_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 21, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate,
                Amount = 120.0d
            });

            var after = CreateDirtyState(accountId: 21, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate,
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([21]);
        }

        [Fact]
        public void Should_Return_Single_Account_When_Both_States_Are_OneTime_And_EndDate_After_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 22, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 11),
                Amount = 120.0d
            });

            var after = CreateDirtyState(accountId: 22, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 11),
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([22]);
        }

        [Fact]
        public void Should_Return_Single_Account_When_Before_Ended_OneTime_And_After_EndDate_Equals_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 23, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 120.0d
            });

            var after = CreateDirtyState(accountId: 23, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate,
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([23]);
        }

        [Fact]
        public void Should_Return_Single_Account_When_Before_EndDate_Equals_AsOfDate_And_After_Ended_OneTime()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 24, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate,
                Amount = 120.0d
            });

            var after = CreateDirtyState(accountId: 24, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([24]);
        }

        [Fact]
        public void Should_Return_Single_Account_When_NonOneTime_And_EndDate_Before_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 25, mutate: state => state with
            {
                Frequency = Frequency.Months,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 120.0d
            });

            var after = CreateDirtyState(accountId: 25, mutate: state => state with
            {
                Frequency = Frequency.Months,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([25]);
        }

        [Fact]
        public void Should_Return_Both_Accounts_When_Reassigned_And_Both_OneTime_EndDate_Equals_AsOfDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 26, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate
            });

            var after = CreateDirtyState(accountId: 27, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = asOfDate
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([26, 27]);
        }

        [Fact]
        public void Should_Return_Empty_When_Reassigned_And_Both_States_Are_Ended_OneTime()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 31, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 8)
            });

            var after = CreateDirtyState(accountId: 32, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 9),
                Amount = 180.0d
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Return_Single_Account_When_Frequency_Changes_To_OneTime_With_Past_Due_And_No_EndDate()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 18, mutate: state => state with
            {
                Frequency = Frequency.Months,
                NextDue = new DateOnly(2026, 2, 9),
                EndDate = null
            });

            var after = CreateDirtyState(accountId: 18, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                NextDue = new DateOnly(2026, 2, 9),
                EndDate = null
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([18]);
        }

        [Fact]
        public void Should_Return_Both_Accounts_When_Reassigned_And_Only_After_State_Is_Ended_OneTime()
        {
            using var context = CreateTestContext();

            var asOfDate = new DateOnly(2026, 2, 10);
            var before = CreateDirtyState(accountId: 19, mutate: state => state with
            {
                Frequency = Frequency.Months,
                EndDate = null
            });

            var after = CreateDirtyState(accountId: 20, mutate: state => state with
            {
                Frequency = Frequency.OneTime,
                EndDate = new DateOnly(2026, 2, 9)
            });

            var accountIds = context.Marker.GetAccountsRequiringRecalc(before, after, asOfDate);

            accountIds.ShouldBe([19, 20]);
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

    public class IsExpenseDeletionImpactful : AccrualDirtyStateManagerFixture
    {
        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public void Should_LogCall_When_Checking_Delete_Impact()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact Logging Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact Logging Expense", 25.0d, "2026-01-01", "2026-02-01", null, Frequency.Months, 1);

            var logContext = context.CaptureLogCalls(() =>
            {
                _ = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 1, 15));
            });

            _ = logContext.ShouldLogCall<AccrualDirtyStateManager>(nameof(AccrualDirtyStateManager.IsExpenseDeletionImpactful));
        }
        */

        [Fact]
        public void Should_Throw_When_Expense_Is_Null()
        {
            using var context = CreateTestContext();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = context.Marker.IsExpenseDeletionImpactful(null!, new DateOnly(2026, 1, 15));
            });

            exception.ParamName.ShouldBe("expense");
        }

        [Fact]
        public void Should_Return_True_When_Not_Excluded_And_Not_Ended()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact Active Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact Active Expense", 25.0d, "2026-01-01", "2026-02-01", "2026-02-15", Frequency.Months, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 14));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Excluded()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact Excluded Account");
            var expense = EntityFactory.CreateExpense(account, true, "Delete Impact Excluded Expense", 25.0d, "2026-01-01", "2026-02-01", null, Frequency.Months, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 1));

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_OneTime_EndDate_Is_Before_AsOfDate()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact OneTime Ended Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact OneTime Ended Expense", 25.0d, "2026-01-01", "2026-02-01", "2026-02-09", Frequency.OneTime, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 10));

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_True_When_OneTime_EndDate_Equals_AsOfDate()   // The expense has not ended until 'tomorrow'
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact OneTime Equal Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact OneTime Equal Expense", 25.0d, "2026-01-01", "2026-02-01", "2026-02-10", Frequency.OneTime, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 10));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_OneTime_EndDate_Is_After_AsOfDate()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact OneTime After Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact OneTime After Expense", 25.0d, "2026-01-01", "2026-02-01", "2026-02-10", Frequency.OneTime, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 9));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_OneTime_EndDate_Is_Null()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact OneTime Null EndDate Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact OneTime Null EndDate Expense", 25.0d, "2026-01-01", "2026-02-01", null, Frequency.OneTime, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 10));

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_Recurring_EndDate_Is_Before_AsOfDate()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Delete Impact Recurring Ended Account");
            var expense = EntityFactory.CreateExpense(account, false, "Delete Impact Recurring Ended Expense", 25.0d, "2026-01-01", "2026-02-01", "2026-02-09", Frequency.Months, 1);

            var result = context.Marker.IsExpenseDeletionImpactful(expense, new DateOnly(2026, 2, 10));

            result.ShouldBeTrue();
        }
    }

    public class SetAccountsDirtyAsync_ByAccountIds : AccrualDirtyStateManagerFixture
    {
        [Fact]
        public async Task Should_Throw_When_AccountIds_Are_Null()
        {
            using var context = CreateTestContext();

            var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await context.Marker.SetAccountsDirtyAsync((IReadOnlyCollection<int>)null!, CancellationToken.None);
            });

            exception.ParamName.ShouldBe("accountIds");
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Marking_Dirty_For_Account()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Logging Branch Account");

            var logContext = await context.CaptureLogCallsAsync(async () =>
            {
                await context.Marker.SetAccountsDirtyAsync([account.Id], CancellationToken.None);
            });

            _ = logContext.ShouldLogCall<AccrualDirtyStateManager>(nameof(AccrualDirtyStateManager.SetAccountsDirtyAsync));
        }
        */

        [Fact]
        public async Task Should_Add_AccountAccrual_When_Missing()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Create Branch Account");

            context.DbContext.AccountAccruals.Count().ShouldBe(0);

            await context.Marker.SetAccountsDirtyAsync([account.Id], CancellationToken.None);

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
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 4, 1)
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountsDirtyAsync([account.Id], CancellationToken.None);

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
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true,
                LastAccruedDate = new DateOnly(2026, 4, 2)
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountsDirtyAsync([account.Id], CancellationToken.None);

            var persistedAccountAccrual = await context.DbContext.AccountAccruals.SingleAsync();

            persistedAccountAccrual.AccruedIsDirty.ShouldBeTrue();
            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Normalize_Duplicate_AccountIds_When_Marking_Dirty()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Duplicate Id Branch Account");

            await context.Marker.SetAccountsDirtyAsync([account.Id, account.Id, account.Id], CancellationToken.None);

            var accountAccrualEntries = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .ToArray();

            accountAccrualEntries.Length.ShouldBe(1);
            accountAccrualEntries.Single().Entity.AccountId.ShouldBe(account.Id);
            accountAccrualEntries.Single().Entity.AccruedIsDirty.ShouldBeTrue();
        }
    }

    public class SetAccountsDirtyAsync_ByExpenses : AccrualDirtyStateManagerFixture
    {
        [Fact]
        public async Task Should_Throw_When_Expenses_Are_Null()
        {
            using var context = CreateTestContext();

            var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await context.Marker.SetAccountsDirtyAsync((IReadOnlyCollection<ExpenseEntity>)null!, CancellationToken.None);
            });

            exception.ParamName.ShouldBe("expenses");
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Marking_Dirty_For_Expenses()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Toggle Logging Account");
            var expenses = new List<ExpenseEntity>
            {
                EntityFactory.CreateExpense(account, false, "Toggle Logging Expense", 15.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1)
            };

            var logContext = await context.CaptureLogCallsAsync(async () =>
            {
                await context.Marker.SetAccountsDirtyAsync(expenses, CancellationToken.None);
            });

            _ = logContext.ShouldLogCall<AccrualDirtyStateManager>(nameof(AccrualDirtyStateManager.SetAccountsDirtyAsync));
        }
        */

        [Fact]
        public async Task Should_Not_Modify_When_Expenses_Are_Empty()
        {
            using var context = CreateTestContext();

            await context.Marker.SetAccountsDirtyAsync(Array.Empty<ExpenseEntity>(), CancellationToken.None);

            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Add_And_Modify_AccountAccruals_For_Affected_Accounts()
        {
            using var context = CreateTestContext();

            var existingCleanAccount = context.AddAccount("Toggle Existing Clean Account");
            var missingAccrualAccount = context.AddAccount("Toggle Missing Accrual Account");
            var existingDirtyAccount = context.AddAccount("Toggle Existing Dirty Account");
            var existingCleanAccountAccrual = new AccountAccrualEntity
            {
                AccountId = existingCleanAccount.Id,
                Account = existingCleanAccount,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 4, 10)
            };
            var existingDirtyAccountAccrual = new AccountAccrualEntity
            {
                AccountId = existingDirtyAccount.Id,
                Account = existingDirtyAccount,
                AccruedIsDirty = true,
                LastAccruedDate = new DateOnly(2026, 4, 11)
            };

            context.DbContext.AccountAccruals.AddRange(
                existingCleanAccountAccrual,
                existingDirtyAccountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            var expenses = new List<ExpenseEntity>
            {
                EntityFactory.CreateExpense(existingCleanAccount, false, "Toggle Existing Clean Expense", 25.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(missingAccrualAccount, false, "Toggle Missing Accrual Expense", 35.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(existingDirtyAccount, false, "Toggle Existing Dirty Expense", 45.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1),
                EntityFactory.CreateExpense(existingCleanAccount, false, "Toggle Existing Clean Expense Duplicate Account", 55.0d, "2026-04-01", "2026-04-30", null, Frequency.Months, 1)
            };

            await context.Marker.SetAccountsDirtyAsync(expenses, CancellationToken.None);

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

    public class SetAccountCleanAsync : AccrualDirtyStateManagerFixture
    {
        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Clearing_Dirty_On_Accrual_Success()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Logging Account");

            var logContext = await context.CaptureLogCallsAsync(async () =>
            {
                await context.Marker.SetAccountCleanAsync(account.Id, new DateOnly(2026, 5, 1), CancellationToken.None);
            });

            _ = logContext.ShouldLogCall<AccrualDirtyStateManager>(nameof(AccrualDirtyStateManager.SetAccountCleanAsync));
        }
        */

        [Fact]
        public async Task Should_Add_AccountAccrual_When_Missing_And_Set_Clean_With_Date()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Missing Account");
            var asOfDate = new DateOnly(2026, 5, 1);

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Added);
            accountAccrualEntry.Entity.AccountId.ShouldBe(account.Id);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeFalse();
            accountAccrualEntry.Entity.LastAccruedDate.ShouldBe(asOfDate);
        }

        [Fact]
        public async Task Should_Update_AccountAccrual_When_Existing_Row_Is_Dirty()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Existing Dirty Account");
            var asOfDate = new DateOnly(2026, 5, 3);
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true,
                LastAccruedDate = new DateOnly(2026, 4, 30)
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeFalse();
            accountAccrualEntry.Entity.LastAccruedDate.ShouldBe(asOfDate);
        }

        [Fact]
        public async Task Should_Update_AccountAccrual_When_Dirty_And_Date_Matches()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Existing Dirty Matching Date Account");
            var asOfDate = new DateOnly(2026, 5, 3);
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true,
                LastAccruedDate = asOfDate
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeFalse();
            accountAccrualEntry.Entity.LastAccruedDate.ShouldBe(asOfDate);
        }

        [Fact]
        public async Task Should_Update_AccountAccrual_When_LastAccruedDate_Differs()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Existing Clean Old Date Account");
            var asOfDate = new DateOnly(2026, 5, 4);
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 5, 1)
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeFalse();
            accountAccrualEntry.Entity.LastAccruedDate.ShouldBe(asOfDate);
        }

        [Fact]
        public async Task Should_Update_AccountAccrual_When_LastAccruedDate_Is_Null()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Existing Clean Null Date Account");
            var asOfDate = new DateOnly(2026, 5, 6);
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = null
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            var accountAccrualEntry = context.DbContext.ChangeTracker
                .Entries<AccountAccrualEntity>()
                .Single();

            accountAccrualEntry.State.ShouldBe(EntityState.Modified);
            accountAccrualEntry.Entity.AccruedIsDirty.ShouldBeFalse();
            accountAccrualEntry.Entity.LastAccruedDate.ShouldBe(asOfDate);
        }

        [Fact]
        public async Task Should_Not_Modify_AccountAccrual_When_Already_Clean_And_Date_Matches()
        {
            using var context = CreateTestContext();

            var account = context.AddAccount("Clear Dirty Existing Clean Current Date Account");
            var asOfDate = new DateOnly(2026, 5, 5);
            var accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = false,
                LastAccruedDate = asOfDate
            };

            context.DbContext.AccountAccruals.Add(accountAccrual);

            await context.DbContext.SaveChangesAsync();
            context.DbContext.ChangeTracker.Clear();

            await context.Marker.SetAccountCleanAsync(account.Id, asOfDate, CancellationToken.None);

            context.DbContext.ChangeTracker.HasChanges().ShouldBeFalse();
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

        var repositoryLogger = NullLogger<AccountAccrualRepository>.Instance;
        var markerLogger = Substitute.For<ILogger<AccrualDirtyStateManager>>();

        var repository = new AccountAccrualRepository(dbContext, repositoryLogger);
        var marker = new AccrualDirtyStateManager(repository, markerLogger);

        return new TestContext(marker, dbContext, site, markerLogger);
    }
}
