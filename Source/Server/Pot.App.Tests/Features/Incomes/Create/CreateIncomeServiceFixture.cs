using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Create;
using Pot.App.Features.Incomes.Create.EntityChecks;
using Pot.App.Features.Incomes.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Create;

public class CreateIncomeServiceFixture : PotFixtureBase
{
    public class CreateIncomeAsync : CreateIncomeServiceFixture
    {
        private readonly IPersistableAccountRepository _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
        private readonly IPreCreateChecker _preCreateCheckerFake = Substitute.For<IPreCreateChecker>();

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = CreateService();
            var input = CreateInput(Guid.NewGuid());

            var result = await service.CreateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            await _preCreateCheckerFake
                .DidNotReceive()
                .CanSaveAsync(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>());
            await _accountRepositoryFake
                .DidNotReceive()
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_When_PreCreateChecker_Returns_Error()
        {
            var account = CreateAccount();
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError(
                "Description", "Salary", "Description must be unique");

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);
            _preCreateCheckerFake
                .CanSaveAsync(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = CreateService();
            var input = CreateInput(account.RowId);

            var result = await service.CreateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe(checkerError);
            await _accountRepositoryFake
                .DidNotReceive()
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Create_Income_When_Request_Is_Valid()
        {
            var account = CreateAccount();

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);
            _preCreateCheckerFake
                .CanSaveAsync(Arg.Any<IncomeEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);
            _accountRepositoryFake
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = CreateService();
            var input = CreateInput(account.RowId);

            var result = await service.CreateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            account.Incomes.ShouldHaveSingleItem();

            var createdIncome = account.Incomes.Single();
            createdIncome.ExcludeFromCalcs.ShouldBe(input.ExcludeFromCalcs);
            createdIncome.Description.ShouldBe(input.Description);
            createdIncome.NextDue.ShouldBe(input.NextDue);
            createdIncome.EndDate.ShouldBe(input.EndDate);
            createdIncome.Frequency.ShouldBe(input.Frequency);
            createdIncome.FrequencyCount.ShouldBe(input.FrequencyCount);
            createdIncome.Amount.ShouldBe(input.Amount);
            createdIncome.Note.ShouldBe(input.Note);
            createdIncome.Account.ShouldBe(account);
            result.Value.RowId.ShouldBe(createdIncome.RowId);

            await _accountRepositoryFake
                .Received(1)
                .UpdateAndSaveAsync(account, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Apply_Input_RowId_When_Provided()
        {
            var account = CreateAccount();
            var providedRowId = Guid.NewGuid();
            IncomeEntity? capturedIncome = null;

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(account);
            _preCreateCheckerFake
                .CanSaveAsync(Arg.Do<IncomeEntity>(income => capturedIncome = income), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);
            _accountRepositoryFake
                .UpdateAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = CreateService();
            var input = CreateInput(account.RowId, providedRowId);

            var result = await service.CreateIncomeAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            capturedIncome.ShouldNotBeNull();
            capturedIncome.RowId.ShouldBe(providedRowId);
        }

        private CreateIncomeService CreateService()
        {
            return new CreateIncomeService(_accountRepositoryFake, _preCreateCheckerFake,
                Substitute.For<ILogger<CreateIncomeService>>());
        }
    }

    private static AccountEntity CreateAccount()
    {
        return EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Income Account", 1000.0d);
    }

    private static Input CreateInput(Guid accountRowId, Guid? rowId = null)
    {
        return new Input
        {
            RowId = rowId,
            AccountRowId = accountRowId,
            ExcludeFromCalcs = true,
            Description = "Salary",
            NextDue = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2027, 5, 1),
            Frequency = Frequency.Months,
            FrequencyCount = 1,
            Amount = 2500.0d,
            Note = "Monthly pay"
        };
    }
}
