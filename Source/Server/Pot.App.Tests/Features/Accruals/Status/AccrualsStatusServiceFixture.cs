using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Accruals.Status;
using Pot.App.Features.Accruals.Status.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Shouldly;

namespace Pot.App.Tests.Features.Accruals.Status;

public class AccrualsStatusServiceFixture
{
    private readonly IExpenseRepository _expenseRepositoryFake;
    private readonly IIncomeRepository _incomeRepositoryFake;

    public AccrualsStatusServiceFixture()
    {
        _expenseRepositoryFake = Substitute.For<IExpenseRepository>();
        _incomeRepositoryFake = Substitute.For<IIncomeRepository>();
    }

    [Fact]
    public void Should_Throw_When_ExpenseRepository_Is_Null()
    {
        var logger = Substitute.For<ILogger<AccrualsStatusService>>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(null!, _incomeRepositoryFake, logger);
        });
    }

    [Fact]
    public void Should_Throw_When_IncomeRepository_Is_Null()
    {
        var logger = Substitute.For<ILogger<AccrualsStatusService>>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(_expenseRepositoryFake, null!, logger);
        });
    }

    [Fact]
    public void Should_Throw_When_Logger_Is_Null()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(_expenseRepositoryFake, _incomeRepositoryFake, null!);
        });
    }

    /*
    TODO(logging): Re-enable when the replacement logging test framework is available.
    [Fact]
    public async Task Should_LogCall_When_Getting_Status()
    {
        var logger = Substitute.For<ILogger<AccrualsStatusService>>();

        var service = new AccrualsStatusService(_accountAccrualRepositoryFake, _expenseRepositoryFake, _incomeRepositoryFake, logger);

        _expenseRepositoryFake
            .GetRequiredRenewalsAsync(Arg.Any<Guid[]>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());

        _accountAccrualRepositoryFake
            .GetRequiredAccountAccrualsAsync(Arg.Any<Guid[]>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());

        _incomeRepositoryFake
            .GetRequiredRenewalsAsync(Arg.Any<Guid[]>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());

        var input = new Input
        {
            AccountRowIds = [],
            AsOfDate = new DateOnly(2026, 4, 24)
        };

        var context = await logger.CaptureLogCallsAsync(async () =>
        {
            _ = await service.GetStatusAsync(input, CancellationToken.None);
        });

        _ = context.ShouldLogCall<AccrualsStatusService>(nameof(AccrualsStatusService.GetStatusAsync));
    }
    */

    [Fact]
    public async Task Should_Get_Required_Renewals_From_The_Renewal_Repositories()
    {
        var service = new AccrualsStatusService(
            _expenseRepositoryFake,
            _incomeRepositoryFake,
            Substitute.For<ILogger<AccrualsStatusService>>());

        var accountRowIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var asOfDate = new DateOnly(2026, 4, 24);

        var expectedExpenseRenewals = new[] { accountRowIds[0] };
        var expectedIncomeRenewals = new[] { accountRowIds[1] };

        _expenseRepositoryFake.GetRequiredRenewalsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>())
            .Returns(expectedExpenseRenewals);

        _incomeRepositoryFake.GetRequiredRenewalsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>())
            .Returns(expectedIncomeRenewals);

        var input = new Input
        {
            AccountRowIds = accountRowIds,
            AsOfDate = asOfDate
        };

        var result = await service.GetStatusAsync(input, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ExpenseRenewalsRequired.ShouldBe(expectedExpenseRenewals);
        result.Value.IncomeRenewalsRequired.ShouldBe(expectedIncomeRenewals);
    }
}
