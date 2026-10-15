using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Incomes.GetAll;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.GetAll;

public class GetIncomesServiceFixture : PotFixtureBase
{
    private readonly IIncomeRepository _incomeRepositoryFake = Substitute.For<IIncomeRepository>();

    [Fact]
    public async Task Should_Map_Incomes_And_Account_Fields()
    {
        var firstAccount = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Primary Account", 1000.0d);
        var secondAccount = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Secondary Account", 2000.0d);
        var firstIncome = EntityFactory.CreateIncome(firstAccount, false, "Salary", 2500.0d, "2026-04-01", null, Frequency.Months, 1);
        var secondIncome = EntityFactory.CreateIncome(secondAccount, true, "Benefit", 500.0d, "2026-04-15", "2027-04-15", Frequency.Weeks, 2);

        _incomeRepositoryFake
            .GetAllIncomesAsync(Arg.Any<CancellationToken>())
            .Returns([firstIncome, secondIncome]);

        var service = CreateService();

        var outputs = await service.GetAllIncomesAsync(TestContext.Current.CancellationToken);
        var byRowId = outputs.ToDictionary(output => output.RowId);

        byRowId.Count.ShouldBe(2);
        AssertMappedIncome(byRowId[firstIncome.RowId], firstIncome, firstAccount);
        AssertMappedIncome(byRowId[secondIncome.RowId], secondIncome, secondAccount);
    }

    [Fact]
    public async Task Should_Return_No_Incomes_When_There_Are_None()
    {
        _incomeRepositoryFake
            .GetAllIncomesAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var service = CreateService();

        var outputs = await service.GetAllIncomesAsync(TestContext.Current.CancellationToken);

        outputs.ShouldBeEmpty();
    }

    private GetIncomesService CreateService()
    {
        return new GetIncomesService(_incomeRepositoryFake, Substitute.For<ILogger<GetIncomesService>>());
    }

    private static void AssertMappedIncome(Pot.App.Features.Incomes.GetAll.Models.Output output, IncomeEntity income, AccountEntity account)
    {
        output.Etag.ShouldBe(income.Etag);
        output.ExcludeFromCalcs.ShouldBe(income.ExcludeFromCalcs);
        output.Description.ShouldBe(income.Description);
        output.NextDue.ShouldBe(income.NextDue);
        output.EndDate.ShouldBe(income.EndDate);
        output.Frequency.ShouldBe(income.Frequency);
        output.FrequencyCount.ShouldBe(income.FrequencyCount);
        output.Amount.ShouldBe(income.Amount);
        output.Note.ShouldBe(income.Note);
        output.Account.RowId.ShouldBe(account.RowId);
        output.Account.Description.ShouldBe(account.Description);
    }
}
