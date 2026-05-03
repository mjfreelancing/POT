using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Accruals.Models;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Update;
using Pot.App.Features.Expenses.Update.EntityChecks;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Update;

public class UpdateExpenseServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class Constructor : UpdateExpenseServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;
        private readonly ITimeProvider _timeProviderFake;

        public Constructor()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
            _timeProviderFake = Substitute.For<ITimeProvider>();
        }

        [Fact]
        public void Should_Throw_When_AccrualDirtyStateManager_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateExpenseService>>();

                _ = new UpdateExpenseService(null!, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accrualDirtyStateManager");
        }

        [Fact]
        public void Should_Throw_When_ExpenseRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateExpenseService>>();

                _ = new UpdateExpenseService(_accrualDirtyStateManagerFake, null!, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("expenseRepository");
        }

        [Fact]
        public void Should_Throw_When_AccountRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateExpenseService>>();

                _ = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, null!, _preUpdateCheckerFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accountRepository");
        }

        [Fact]
        public void Should_Throw_When_PreUpdateChecker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateExpenseService>>();

                _ = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, null!, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("preUpdateChecker");
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateExpenseService>>();

                _ = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, null!, logger);
            });

            exception.ParamName.ShouldBe("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class UpdateExpenseAsync : UpdateExpenseServiceFixture
    {
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPersistableExpenseRepository _expenseRepositoryFake;
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;
        private readonly ITimeProvider _timeProviderFake;

        public UpdateExpenseAsync()
        {
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _expenseRepositoryFake = Substitute.For<IPersistableExpenseRepository>();
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
            _timeProviderFake = Substitute.For<ITimeProvider>();
            _expenseRepositoryFake.WithTracking().Returns(new NoopScope());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogCall_When_Updating_Expense()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((ExpenseEntity?)null);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(Guid.NewGuid(), Guid.NewGuid());
            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.UpdateExpenseAsync(input, CancellationToken.None);
            });

            _ = context.ShouldLogCall<UpdateExpenseService>(nameof(UpdateExpenseService.UpdateExpenseAsync));
        }
        */

        [Fact]
        public async Task Should_Fail_When_Expense_Does_Not_Exist()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((ExpenseEntity?)null);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(Guid.NewGuid(), Guid.NewGuid());

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _accountRepositoryFake
                .DidNotReceive()
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

            await _preUpdateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogApiError_When_Expense_Does_Not_Exist()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((ExpenseEntity?)null);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(Guid.NewGuid(), Guid.NewGuid());
            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.UpdateExpenseAsync(input, CancellationToken.None);
            });

            _ = context.ShouldLogAtLevel<UpdateExpenseService>(LogLevel.Information, "The expense does not exist");
        }
        */

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var existingExpense = CreateExpense(accountId: 10);

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, Guid.NewGuid());

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _preUpdateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogApiError_When_Account_Does_Not_Exist()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var existingExpense = CreateExpense(accountId: 10);

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, Guid.NewGuid());
            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.UpdateExpenseAsync(input, CancellationToken.None);
            });

            _ = context.ShouldLogAtLevel<UpdateExpenseService>(LogLevel.Information, "The account does not exist");
        }
        */

        [Fact]
        public async Task Should_Fail_When_PreUpdateChecker_Returns_Error()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var existingExpense = CreateExpense(accountId: 10);
            var targetAccount = CreateAccount(accountId: 12);
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError("Description", "Updated expense", "Description must be unique");

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, targetAccount.RowId);

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            _accrualDirtyStateManagerFake
                .DidNotReceive()
                .GetAccountsRequiringRecalc(Arg.Any<ExpenseAccrualState>(), Arg.Any<ExpenseAccrualState>(), Arg.Any<DateOnly>());

            await _expenseRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        /*
        TODO(logging): Re-enable when the replacement logging test framework is available.
        [Fact]
        public async Task Should_LogApiError_When_PreUpdateChecker_Returns_Validation_Error()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var existingExpense = CreateExpense(accountId: 10);
            var targetAccount = CreateAccount(accountId: 12);
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError("Description", "Updated expense", "Description must be unique");

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, targetAccount.RowId);
            var context = await logger.CaptureLogCallsAsync(async () =>
            {
                _ = await service.UpdateExpenseAsync(input, CancellationToken.None);
            });

            _ = context.ShouldLogAtLevel<UpdateExpenseService>(LogLevel.Information, "Description must be unique");
        }
        */

        [Fact]
        public async Task Should_Update_Expense_And_Mark_Relevant_Accounts_Dirty_When_Request_Is_Valid()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var sourceAccount = CreateAccount(accountId: 40);
            var targetAccount = CreateAccount(accountId: 41);
            var existingExpense = CreateExpense(accountId: sourceAccount.Id, account: sourceAccount);
            var accountIdsToMarkDirty = new[] { sourceAccount.Id, targetAccount.Id };
            var localDate = new DateOnly(2026, 4, 24);
            ExpenseAccrualState? beforeState = null;
            ExpenseAccrualState? afterState = null;
            IReadOnlyCollection<int>? markedAccountIds = null;

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);

            _timeProviderFake.GetLocalDateNow().Returns(localDate);

            _accrualDirtyStateManagerFake
                .GetAccountsRequiringRecalc(
                    Arg.Do<ExpenseAccrualState>(state => beforeState = state),
                    Arg.Do<ExpenseAccrualState>(state => afterState = state),
                    Arg.Is<DateOnly>(date => date == localDate))
                .Returns(accountIdsToMarkDirty);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<int>>(ids => markedAccountIds = ids), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, targetAccount.RowId);

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            var output = result.Value!;
            output.RowId.ShouldBe(existingExpense.RowId);

            existingExpense.Description.ShouldBe(input.Description);
            existingExpense.Account.ShouldBe(targetAccount);

            beforeState.ShouldNotBeNull();
            var before = beforeState!;
            before.AccountId.ShouldBe(sourceAccount.Id);

            afterState.ShouldNotBeNull();
            var after = afterState!;
            after.AccountId.ShouldBe(targetAccount.Id);
            after.Amount.ShouldBe(input.Amount);

            markedAccountIds.ShouldNotBeNull();
            markedAccountIds.ToArray().ShouldBe(accountIdsToMarkDirty);

            await _expenseRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Propagate_Empty_AccountsToMarkDirty_Result()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var sourceAccount = CreateAccount(accountId: 42);
            var targetAccount = CreateAccount(accountId: 43);
            var existingExpense = CreateExpense(accountId: sourceAccount.Id, account: sourceAccount);
            IReadOnlyCollection<int>? markedAccountIds = null;

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);

            _timeProviderFake.GetLocalDateNow().Returns(new DateOnly(2026, 4, 24));

            _accrualDirtyStateManagerFake
                .GetAccountsRequiringRecalc(Arg.Any<ExpenseAccrualState>(), Arg.Any<ExpenseAccrualState>(), Arg.Any<DateOnly>())
                .Returns(Array.Empty<int>());

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<int>>(ids => markedAccountIds = ids), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, targetAccount.RowId);

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            markedAccountIds.ShouldNotBeNull();
            markedAccountIds.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_Propagate_Single_Account_To_Mark_Dirty_Result()
        {
            var logger = Substitute.For<ILogger<UpdateExpenseService>>();

            var sourceAccount = CreateAccount(accountId: 44);
            var existingExpense = CreateExpense(accountId: sourceAccount.Id, account: sourceAccount);
            var accountIdsToMarkDirty = new[] { sourceAccount.Id };
            IReadOnlyCollection<int>? markedAccountIds = null;

            _expenseRepositoryFake
                .GetExpenseOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingExpense);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(sourceAccount);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);

            _timeProviderFake.GetLocalDateNow().Returns(new DateOnly(2026, 4, 24));

            _accrualDirtyStateManagerFake
                .GetAccountsRequiringRecalc(Arg.Any<ExpenseAccrualState>(), Arg.Any<ExpenseAccrualState>(), Arg.Any<DateOnly>())
                .Returns(accountIdsToMarkDirty);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<int>>(ids => markedAccountIds = ids), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _expenseRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new UpdateExpenseService(_accrualDirtyStateManagerFake, _expenseRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(existingExpense.RowId, sourceAccount.RowId);

            var result = await service.UpdateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            markedAccountIds.ShouldNotBeNull();
            markedAccountIds.ToArray().ShouldBe(accountIdsToMarkDirty);
        }
    }

    private static AccountEntity CreateAccount(int accountId)
    {
        var site = EntityFactory.CreateSite();

        return new AccountEntity
        {
            Id = accountId,
            RowId = Guid.NewGuid(),
            Bsb = "123-456",
            Number = "12345678",
            Description = $"Account-{accountId}",
            Balance = 1000.0d,
            Reserved = 0.0d,
            Site = site,
            Expenses = [],
            Incomes = []
        };
    }

    private static ExpenseEntity CreateExpense(int accountId, AccountEntity? account = null)
    {
        var ownerAccount = account ?? CreateAccount(accountId);

        return new ExpenseEntity
        {
            RowId = Guid.NewGuid(),
            Etag = 5,
            ExcludeFromCalcs = false,
            Description = "Original expense",
            Amount = 80.0d,
            AccrualStart = new DateOnly(2026, 4, 1),
            NextDue = new DateOnly(2026, 5, 1),
            EndDate = null,
            AccrualPolicy = AccrualPolicy.Automatic,
            Frequency = Frequency.Months,
            FrequencyCount = 1,
            Account = ownerAccount
        };
    }

    private static Input CreateInput(Guid expenseRowId, Guid accountRowId)
    {
        return new Input
        {
            RowId = expenseRowId,
            Etag = 5,
            AccountRowId = accountRowId,
            ExcludeFromCalcs = false,
            Description = "Updated expense",
            AccrualStart = new DateOnly(2026, 4, 15),
            NextDue = new DateOnly(2026, 5, 15),
            EndDate = null,
            AccrualPolicy = AccrualPolicy.Automatic,
            Frequency = Frequency.Months,
            FrequencyCount = 2,
            Amount = 120.0d,
            Note = "Updated note"
        };
    }
}
