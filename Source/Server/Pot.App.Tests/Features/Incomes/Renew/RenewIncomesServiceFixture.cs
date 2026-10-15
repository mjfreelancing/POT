using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Features.Incomes.Renew;
using Pot.App.Features.Incomes.Renew.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Incomes.Renew;

public class RenewIncomesServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class Constructor : RenewIncomesServiceFixture
    {
        private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();
        private readonly IIncomeRenewalCalculator _renewalCalculatorFake = Substitute.For<IIncomeRenewalCalculator>();

        [Fact]
        public void Should_Throw_When_IncomeRepository_Is_Null()
        {
            var logger = Substitute.For<ILogger<RenewIncomesService>>();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new RenewIncomesService(null!, _renewalCalculatorFake, logger);
            });

            exception.ParamName.ShouldBe("incomeRepository");
        }

        [Fact]
        public void Should_Throw_When_RenewalCalculator_Is_Null()
        {
            var logger = Substitute.For<ILogger<RenewIncomesService>>();

            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new RenewIncomesService(_incomeRepositoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("renewalCalculator");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new RenewIncomesService(_incomeRepositoryFake, _renewalCalculatorFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class RenewAsync : RenewIncomesServiceFixture
    {
        private readonly IPersistableIncomeRepository _incomeRepositoryFake = Substitute.For<IPersistableIncomeRepository>();
        private readonly IIncomeRenewalCalculator _renewalCalculatorFake = Substitute.For<IIncomeRenewalCalculator>();

        public RenewAsync()
        {
            _incomeRepositoryFake.WithTracking().Returns(new NoopScope());
        }

        [Fact]
        public async Task Should_Fail_When_One_Or_More_Incomes_Do_Not_Exist()
        {
            var logger = Substitute.For<ILogger<RenewIncomesService>>();
            var existingIncome = CreateIncome("Existing Income");
            var missingRowId = Guid.NewGuid();

            _incomeRepositoryFake
                .GetIncomesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([existingIncome]);

            var service = new RenewIncomesService(_incomeRepositoryFake, _renewalCalculatorFake, logger);
            var input = CreateInput([existingIncome.RowId, missingRowId]);

            var result = await service.RenewAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();

            _renewalCalculatorFake
                .DidNotReceive()
                .Renew(Arg.Any<IEnumerable<IncomeEntity>>(), Arg.Any<RenewalMode>(), Arg.Any<DateOnly>());

            await _incomeRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Renew_Incomes_And_Save_Changes()
        {
            var logger = Substitute.For<ILogger<RenewIncomesService>>();
            var firstIncome = CreateIncome("First Income");
            var secondIncome = CreateIncome("Second Income");

            _incomeRepositoryFake
                .GetIncomesAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
                .Returns([firstIncome, secondIncome]);

            var service = new RenewIncomesService(_incomeRepositoryFake, _renewalCalculatorFake, logger);
            var rowIds = new[] { firstIncome.RowId, secondIncome.RowId };
            var input = CreateInput(rowIds);

            var result = await service.RenewAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();

            await _incomeRepositoryFake.Received(1).GetIncomesAsync(rowIds, Arg.Any<CancellationToken>());

            _renewalCalculatorFake
                .Received(1)
                .Renew(Arg.Is<IEnumerable<IncomeEntity>>(incomes => incomes.SequenceEqual(new[] { firstIncome, secondIncome })),
                    RenewalMode.Overdue, input.AsOfDate);

            await _incomeRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }

    private static IncomeEntity CreateIncome(string description)
    {
        var site = EntityFactory.CreateSite();
        var account = EntityFactory.CreateAccount(site, "Income Account", 1000.0d);

        return EntityFactory.CreateIncome(account, false, description, 100.0d, "2026-04-01", null, Frequency.Months, 1);
    }

    private static Input CreateInput(Guid[] rowIds)
    {
        return new Input
        {
            RowIds = rowIds,
            AsOfDate = new DateOnly(2026, 4, 24),
            Mode = RenewalMode.Overdue
        };
    }
}
