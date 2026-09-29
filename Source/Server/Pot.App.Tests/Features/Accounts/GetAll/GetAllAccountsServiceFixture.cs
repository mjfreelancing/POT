using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Accounts.GetAll;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.GetAll;

public class GetAllAccountsServiceFixture : PotFixtureBase
{
    private static readonly DateOnly Today = new(2025, 1, 15);

    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();

    public GetAllAccountsServiceFixture()
    {
        _timeProvider.GetLocalDateNow().Returns(Today);
    }

    [Fact]
    public async Task Should_Compose_The_Accrual_Members_From_The_Calculation()
    {
        // A representative mix on a 1000 balance with 100 reserved:
        // - a 100 monthly bill due today, so it accrues in full;
        // - a 70 weekly bill due on 2025-01-01 with two occurrences past due, so it carries 140 of arrears while its
        //   cycle in progress is itself due today;
        // - a 100 monthly bill due on 2025-02-15, so it only ramps.
        var account = CreateAccount(
        [
            CreateBill(amount: 100.0d, nextDue: "2025-01-15", accrualStart: "2025-01-15", frequency: Frequency.Months),
            CreateBill(amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01", frequency: Frequency.Weeks),
            CreateBill(amount: 100.0d, nextDue: "2025-02-15", accrualStart: "2025-01-15", frequency: Frequency.Months)
        ]);

        var output = await GetSingleAccountOutputAsync(account);

        output.TotalExpenseAccrued.ShouldBe(170.0d, "100 due today plus the weekly cycle in progress at its full amount");
        output.TotalArrears.ShouldBe(140.0d, "two past-due weekly occurrences at 70 each");
        output.TotalCommitted.ShouldBe(310.0d);
        output.StableExpenseAccrual.ShouldBe(16.5710d, 0.001d);
    }

    [Fact]
    public async Task Should_Compose_Available_From_Balance_Reserved_And_Committed()
    {
        var account = CreateAccount(
            [CreateBill(amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01", frequency: Frequency.Weeks)]);

        var output = await GetSingleAccountOutputAsync(account);

        // 1000 balance - 100 reserved - 70 accrued - 140 arrears = 690.
        output.Available.ShouldBe(690.0d);
        output.Available.ShouldBe(output.Balance - output.Reserved - output.TotalCommitted);
    }

    [Fact]
    public async Task Should_Pass_The_Account_Fields_Through()
    {
        var account = CreateAccount(
            [CreateBill(amount: 100.0d, nextDue: "2025-01-15", accrualStart: "2025-01-15", frequency: Frequency.Months)],
            linkedIncomes: 2);

        var output = await GetSingleAccountOutputAsync(account);

        output.RowId.ShouldBe(account.Account.RowId);
        output.Etag.ShouldBe(account.Account.Etag);
        output.Description.ShouldBe(account.Account.Description);
        output.Balance.ShouldBe(account.Account.Balance);
        output.Reserved.ShouldBe(account.Account.Reserved);
        output.LinkedExpenses.ShouldBe(1);
        output.LinkedIncomes.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Return_No_Accounts_When_There_Are_None()
    {
        _accountRepository
            .GetAllAccountsWithLinkedCountsAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var accounts = await CreateService().GetAllAccountsAsync(TestContext.Current.CancellationToken);

        accounts.ShouldBeEmpty();
    }

    private async Task<Output> GetSingleAccountOutputAsync(AccountWithLinkedCounts account)
    {
        _accountRepository
            .GetAllAccountsWithLinkedCountsAsync(Arg.Any<CancellationToken>())
            .Returns([account]);

        var accounts = await CreateService().GetAllAccountsAsync(TestContext.Current.CancellationToken);

        return accounts.ShouldHaveSingleItem();
    }

    private GetAllAccountsService CreateService()
    {
        return new GetAllAccountsService(_accountRepository, new AccrualCalculator(), _timeProvider,
            NullLogger<GetAllAccountsService>.Instance);
    }

    /// <summary>Builds the account read model with the accrual facts projected exactly as the query does.</summary>
    private static AccountWithLinkedCounts CreateAccount(ExpenseEntity[] expenses, int linkedIncomes = 0)
    {
        var site = EntityFactory.CreateSite();
        var account = EntityFactory.CreateAccount(site, "Everyday", balance: 1000.0d, reserved: 100.0d);

        return new AccountWithLinkedCounts
        {
            Account = account,
            LinkedExpenses = expenses.Length,
            LinkedIncomes = linkedIncomes,
            AccrualExpenses = [.. expenses.Select(expense => expense.MapToAccrualInput())]
        };
    }

    private static ExpenseEntity CreateBill(double amount, string nextDue, string accrualStart, Frequency frequency)
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Everyday", 1000.0d);

        return EntityFactory.CreateExpense(account, excludeFromCalc: false, "Bill", amount, accrualStart, nextDue,
            endDate: null, frequency, frequencyCount: 1);
    }
}
