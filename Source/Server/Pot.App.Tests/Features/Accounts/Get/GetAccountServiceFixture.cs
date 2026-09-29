using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Get;
using Pot.App.Mappings;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.Get;

public class GetAccountServiceFixture : PotFixtureBase
{
    private static readonly DateOnly Today = new(2025, 1, 15);

    private readonly IPersistableAccountRepository _accountRepository = Substitute.For<IPersistableAccountRepository>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();

    public GetAccountServiceFixture()
    {
        _timeProvider.GetLocalDateNow().Returns(Today);
    }

    [Fact]
    public async Task Should_Fail_When_The_Account_Does_Not_Exist()
    {
        var accountRowId = Guid.NewGuid();

        _accountRepository
            .GetAccountWithLinkedCountsOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AccountWithLinkedCounts?)null);

        var result = await CreateService().GetAccountWithLinkedCountsAsync(accountRowId, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        _ = result.Error.ShouldBeOfType<ApiDetailError>();
    }

    [Fact]
    public async Task Should_Compose_The_Accrual_Members_From_The_Calculation()
    {
        var site = EntityFactory.CreateSite();
        var account = EntityFactory.CreateAccount(site, "Everyday", balance: 1000.0d, reserved: 100.0d);
        var bill = EntityFactory.CreateExpense(account, excludeFromCalc: false, "Bill", 100.0d, "2025-01-15", "2025-01-15",
            endDate: null, Frequency.Months, frequencyCount: 1);

        var dto = new AccountWithLinkedCounts
        {
            Account = account,
            LinkedExpenses = 1,
            LinkedIncomes = 0,
            AccrualExpenses = [bill.MapToAccrualInput()]
        };

        _accountRepository
            .GetAccountWithLinkedCountsOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        var result = await CreateService().GetAccountWithLinkedCountsAsync(account.RowId, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();

        var output = result.Value!;

        output.TotalExpenseAccrued.ShouldBe(100.0d, "a bill due today accrues in full");
        output.TotalArrears.ShouldBe(0.0d);
        output.TotalCommitted.ShouldBe(100.0d);
        output.Available.ShouldBe(800.0d);
    }

    private GetAccountService CreateService()
    {
        return new GetAccountService(_accountRepository, new AccrualCalculator(), _timeProvider,
            NullLogger<GetAccountService>.Instance);
    }
}
