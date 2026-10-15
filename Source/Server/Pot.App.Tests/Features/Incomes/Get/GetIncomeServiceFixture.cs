using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Incomes.Get;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Get;

public class GetIncomeServiceFixture : PotFixtureBase
{
    private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();

    [Fact]
    public async Task Should_Return_Null_When_The_Income_Does_Not_Exist()
    {
        _incomeRepositoryFake
            .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((IncomeEntity?)null);

        var service = CreateService();

        var result = await service.GetIncomeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Map_Income_And_Account_Fields()
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Income Account", 1000.0d);
        var income = EntityFactory.CreateIncome(account, true, "Salary", 2500.0d, "2026-04-01", "2027-04-01", Frequency.Months, 1);

        _incomeRepositoryFake
            .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(income);

        var service = CreateService();

        var result = await service.GetIncomeAsync(income.RowId, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.RowId.ShouldBe(income.RowId);
        result.Etag.ShouldBe(income.Etag);
        result.ExcludeFromCalcs.ShouldBe(income.ExcludeFromCalcs);
        result.Description.ShouldBe(income.Description);
        result.NextDue.ShouldBe(income.NextDue);
        result.EndDate.ShouldBe(income.EndDate);
        result.Frequency.ShouldBe(income.Frequency);
        result.FrequencyCount.ShouldBe(income.FrequencyCount);
        result.Amount.ShouldBe(income.Amount);
        result.Note.ShouldBe(income.Note);
        result.Account.RowId.ShouldBe(account.RowId);
        result.Account.Description.ShouldBe(account.Description);
    }

    private GetIncomeService CreateService()
    {
        return new GetIncomeService(_incomeRepositoryFake, Substitute.For<ILogger<GetIncomeService>>());
    }
}
