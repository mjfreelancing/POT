using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Incomes.Delete;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Delete;

public class DeleteIncomeServiceFixture : PotFixtureBase
{
    private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();

    [Fact]
    public async Task Should_Return_False_When_Income_Does_Not_Exist()
    {
        _incomeRepositoryFake
            .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((IncomeEntity?)null);

        var service = CreateService();

        var result = await service.DeleteIncomeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
        _incomeRepositoryFake.DidNotReceive().Delete(Arg.Any<IncomeEntity>());
        await _incomeRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Delete_Income_And_Save_Changes_When_It_Exists()
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Income Account", 1000.0d);
        var income = EntityFactory.CreateIncome(account, false, "Salary", 2500.0d, "2026-04-01", null, Frequency.Months, 1);

        _incomeRepositoryFake
            .GetIncomeOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(income);
        _incomeRepositoryFake
            .SaveAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var service = CreateService();

        var result = await service.DeleteIncomeAsync(income.RowId, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
        _incomeRepositoryFake.Received(1).Delete(income);
        await _incomeRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    private DeleteIncomeService CreateService()
    {
        return new DeleteIncomeService(_incomeRepositoryFake, Substitute.For<ILogger<DeleteIncomeService>>());
    }
}
