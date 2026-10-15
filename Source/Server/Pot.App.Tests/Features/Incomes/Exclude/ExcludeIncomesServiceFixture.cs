using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Incomes.Exclude;
using Pot.App.Features.Incomes.Exclude.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Exclude;

public class ExcludeIncomesServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();

    public ExcludeIncomesServiceFixture()
    {
        _incomeRepositoryFake.WithTracking().Returns(new NoopScope());
    }

    [Fact]
    public async Task Should_Fail_When_One_Or_More_Incomes_Do_Not_Exist()
    {
        var existingIncome = CreateIncome("Existing Income", excluded: false);
        var missingRowId = Guid.NewGuid();

        _incomeRepositoryFake
            .GetIncomesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns([existingIncome]);

        var service = CreateService();
        var input = new Input { RowIds = [existingIncome.RowId, missingRowId] };

        var result = await service.ToggleExclusionAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        existingIncome.ExcludeFromCalcs.ShouldBeFalse();
        await _incomeRepositoryFake
            .DidNotReceive()
            .SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Toggle_Exclusion_When_All_Incomes_Exist()
    {
        var firstIncome = CreateIncome("First Income", excluded: false);
        var secondIncome = CreateIncome("Second Income", excluded: true);
        var rowIds = new[] { firstIncome.RowId, secondIncome.RowId };

        _incomeRepositoryFake
            .GetIncomesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns([firstIncome, secondIncome]);
        _incomeRepositoryFake
            .SaveAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var service = CreateService();
        var input = new Input { RowIds = rowIds };

        var result = await service.ToggleExclusionAsync(input, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        firstIncome.ExcludeFromCalcs.ShouldBeTrue();
        secondIncome.ExcludeFromCalcs.ShouldBeFalse();
        await _incomeRepositoryFake.Received(1).GetIncomesAsync(rowIds, Arg.Any<CancellationToken>());
        await _incomeRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    private ExcludeIncomesService CreateService()
    {
        return new ExcludeIncomesService(_incomeRepositoryFake, Substitute.For<ILogger<ExcludeIncomesService>>());
    }

    private static IncomeEntity CreateIncome(string description, bool excluded)
    {
        var account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Income Account", 1000.0d);

        return EntityFactory.CreateIncome(account, excluded, description, 100.0d, "2026-04-01", null, Frequency.Months, 1);
    }
}
