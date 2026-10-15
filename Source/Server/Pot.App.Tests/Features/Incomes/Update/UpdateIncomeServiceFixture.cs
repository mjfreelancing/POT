using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Update;
using Pot.App.Features.Incomes.Update.EntityChecks;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Update;

public class UpdateIncomeServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class UpdateIncomeAsync : UpdateIncomeServiceFixture
    {
        private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();
        private readonly IPersistableAccountRepository _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
        private readonly IPreUpdateChecker _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();

        public UpdateIncomeAsync()
        {
            _incomeRepositoryFake.WithTracking().Returns(new NoopScope());
        }

        [Fact]
        public async Task Should_Fail_When_Income_Does_Not_Exist()
        {
            _incomeRepositoryFake
                .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((IncomeEntity?)null);

            var service = CreateService();
            var input = CreateInput(Guid.NewGuid(), Guid.NewGuid());

            var result = await service.UpdateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            await _accountRepositoryFake
                .DidNotReceive()
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _preUpdateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            var existingIncome = CreateIncome(CreateAccount());

            _incomeRepositoryFake
                .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingIncome);
            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = CreateService();
            var input = CreateInput(existingIncome.RowId, Guid.NewGuid());

            var result = await service.UpdateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            await _preUpdateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_When_PreUpdateChecker_Returns_Error()
        {
            var existingIncome = CreateIncome(CreateAccount());
            var targetAccount = CreateAccount();
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError(
                "Description", "Updated salary", "Description must be unique");

            _incomeRepositoryFake
                .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingIncome);
            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);
            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = CreateService();
            var input = CreateInput(existingIncome.RowId, targetAccount.RowId);

            var result = await service.UpdateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe(checkerError);
            await _incomeRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Update_Income_When_Request_Is_Valid()
        {
            var sourceAccount = CreateAccount();
            var targetAccount = CreateAccount();
            var existingIncome = CreateIncome(sourceAccount);

            _incomeRepositoryFake
                .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingIncome);
            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(targetAccount);
            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);
            _incomeRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var service = CreateService();
            var input = CreateInput(existingIncome.RowId, targetAccount.RowId);

            var result = await service.UpdateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value!.RowId.ShouldBe(existingIncome.RowId);
            existingIncome.ExcludeFromCalcs.ShouldBe(input.ExcludeFromCalcs);
            existingIncome.Description.ShouldBe(input.Description);
            existingIncome.NextDue.ShouldBe(input.NextDue);
            existingIncome.EndDate.ShouldBe(input.EndDate);
            existingIncome.Frequency.ShouldBe(input.Frequency);
            existingIncome.FrequencyCount.ShouldBe(input.FrequencyCount);
            existingIncome.Amount.ShouldBe(input.Amount);
            existingIncome.Note.ShouldBe(input.Note);
            existingIncome.Account.ShouldBe(targetAccount);

            await _incomeRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        private UpdateIncomeService CreateService()
        {
            return new UpdateIncomeService(_incomeRepositoryFake, _accountRepositoryFake, _preUpdateCheckerFake,
                Substitute.For<ILogger<UpdateIncomeService>>());
        }
    }

    private static AccountEntity CreateAccount()
    {
        return EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Income Account", 1000.0d);
    }

    private static IncomeEntity CreateIncome(AccountEntity account)
    {
        return EntityFactory.CreateIncome(account, false, "Original salary", 2000.0d, "2026-04-01", null, Frequency.Months, 1);
    }

    private static Input CreateInput(Guid incomeRowId, Guid accountRowId)
    {
        return new Input
        {
            RowId = incomeRowId,
            Etag = 3,
            AccountRowId = accountRowId,
            ExcludeFromCalcs = true,
            Description = "Updated salary",
            NextDue = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2027, 5, 1),
            Frequency = Frequency.Weeks,
            FrequencyCount = 2,
            Amount = 3000.0d,
            Note = "Updated pay"
        };
    }
}
