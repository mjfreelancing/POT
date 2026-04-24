using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.App.Concerns.Accruals;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Create;
using Pot.App.Features.Expenses.Create.EntityChecks;
using Pot.App.Features.Expenses.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;

namespace Pot.App.Tests.Features.Expenses.Create;

public class CreateExpenseServiceFixture : PotFixtureBase
{
    public class Constructor : CreateExpenseServiceFixture
    {
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPreCreateChecker _preCreateCheckerFake;
        private readonly ITimeProvider _timeProviderFake;

        public Constructor()
        {
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _preCreateCheckerFake = Substitute.For<IPreCreateChecker>();
            _timeProviderFake = Substitute.For<ITimeProvider>();
        }

        [Fact]
        public void Should_Throw_When_AccountRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<CreateExpenseService>>();

                _ = new CreateExpenseService(null!, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accountRepository");
        }

        [Fact]
        public void Should_Throw_When_AccrualDirtyStateManager_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<CreateExpenseService>>();

                _ = new CreateExpenseService(_accountRepositoryFake, null!, _preCreateCheckerFake, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("accrualDirtyStateManager");
        }

        [Fact]
        public void Should_Throw_When_PreCreateChecker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<CreateExpenseService>>();

                _ = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, null!, _timeProviderFake, logger);
            });

            exception.ParamName.ShouldBe("preCreateChecker");
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<CreateExpenseService>>();

                _ = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, null!, logger);
            });

            exception.ParamName.ShouldBe("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class CreateExpenseAsync : CreateExpenseServiceFixture
    {
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly IAccrualDirtyStateManager _accrualDirtyStateManagerFake;
        private readonly IPreCreateChecker _preCreateCheckerFake;
        private readonly ITimeProvider _timeProviderFake;

        public CreateExpenseAsync()
        {
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _accrualDirtyStateManagerFake = Substitute.For<IAccrualDirtyStateManager>();
            _preCreateCheckerFake = Substitute.For<IPreCreateChecker>();
            _timeProviderFake = Substitute.For<ITimeProvider>();
        }

        [Fact]
        public async Task Should_LogCall_When_Creating_Expense()
        {
            var logCollector = new FakeLogCollector();
            var logger = new FakeLogger<CreateExpenseService>(logCollector);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);

            _ = await service.CreateExpenseAsync(CreateInput(Guid.NewGuid()), CancellationToken.None);

            logCollector.ShouldContainLogCall(
                category: typeof(CreateExpenseService).FullName!,
                callerName: nameof(CreateExpenseService.CreateExpenseAsync),
                callerType: typeof(CreateExpenseService));
        }

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            var logger = Substitute.For<ILogger<CreateExpenseService>>();

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(accountRowId: Guid.NewGuid());

            var result = await service.CreateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _preCreateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>());

            await _accrualDirtyStateManagerFake
                .DidNotReceive()
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_When_PreCreateChecker_Returns_Error()
        {
            var logger = Substitute.For<ILogger<CreateExpenseService>>();

            var account = CreateAccount(accountId: 11);
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError("Description", "Rent", "Description must be unique");

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _preCreateCheckerFake
                .CanSaveAsync(Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(accountRowId: account.RowId);

            var result = await service.CreateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _accrualDirtyStateManagerFake
                .DidNotReceive()
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());

            await _accountRepositoryFake
                .DidNotReceive()
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Create_Expense_And_Mark_Account_Dirty_When_Request_Is_Valid()
        {
            var logger = Substitute.For<ILogger<CreateExpenseService>>();

            var account = CreateAccount(accountId: 17);
            var localDate = new DateOnly(2026, 4, 24);
            var markedAccountIds = Array.Empty<int>();

            _timeProviderFake.GetLocalDateNow().Returns(localDate);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _preCreateCheckerFake
                .CanSaveAsync(Arg.Any<ExpenseEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Do<IReadOnlyCollection<int>>(ids => markedAccountIds = ids.ToArray()), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _accountRepositoryFake
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(accountRowId: account.RowId);

            var result = await service.CreateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            var output = result.Value!;
            output.AccrualStart.ShouldNotBeNull();

            account.Expenses.Count.ShouldBe(1);

            var createdExpense = account.Expenses.Single();
            createdExpense.Description.ShouldBe(input.Description);
            createdExpense.Account.ShouldBe(account);

            markedAccountIds.ShouldBe([account.Id]);

            await _accountRepositoryFake
                .Received(1)
                .UpdateAndSaveAsync(account, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Apply_Input_RowId_When_Provided()
        {
            var logger = Substitute.For<ILogger<CreateExpenseService>>();

            var account = CreateAccount(accountId: 23);
            var providedRowId = Guid.NewGuid();
            ExpenseEntity? capturedExpense = null;

            _timeProviderFake.GetLocalDateNow().Returns(new DateOnly(2026, 4, 24));

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);

            _preCreateCheckerFake
                .CanSaveAsync(Arg.Do<ExpenseEntity>(expense => capturedExpense = expense), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);

            _accrualDirtyStateManagerFake
                .SetAccountsDirtyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _accountRepositoryFake
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new CreateExpenseService(_accountRepositoryFake, _accrualDirtyStateManagerFake, _preCreateCheckerFake, _timeProviderFake, logger);

            var input = CreateInput(accountRowId: account.RowId);
            input = new Input
            {
                RowId = providedRowId,
                AccountRowId = input.AccountRowId,
                ExcludeFromCalcs = input.ExcludeFromCalcs,
                Description = input.Description,
                AccrualStart = input.AccrualStart,
                NextDue = input.NextDue,
                EndDate = input.EndDate,
                AccrualPolicy = input.AccrualPolicy,
                Frequency = input.Frequency,
                FrequencyCount = input.FrequencyCount,
                Amount = input.Amount,
                Note = input.Note
            };

            var result = await service.CreateExpenseAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            capturedExpense.ShouldNotBeNull();
            capturedExpense.RowId.ShouldBe(providedRowId);
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
            Description = "Test account",
            Balance = 1000.0d,
            Reserved = 0.0d,
            Site = site,
            Expenses = [],
            Incomes = []
        };
    }

    private static Input CreateInput(Guid accountRowId)
    {
        return new Input
        {
            AccountRowId = accountRowId,
            ExcludeFromCalcs = false,
            Description = "Internet",
            AccrualStart = new DateOnly(2026, 4, 1),
            NextDue = new DateOnly(2026, 5, 1),
            EndDate = null,
            AccrualPolicy = AccrualPolicy.Automatic,
            Frequency = Frequency.Months,
            FrequencyCount = 1,
            Amount = 89.95d,
            Note = "Monthly invoice"
        };
    }
}
