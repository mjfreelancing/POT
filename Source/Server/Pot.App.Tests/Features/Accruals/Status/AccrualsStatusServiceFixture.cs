using NSubstitute;
using Microsoft.Extensions.Logging.Testing;
using Pot.App.Features.Accruals.Status;
using Pot.App.Features.Accruals.Status.Models;
using Pot.Data.Repositories.AccountAccrual;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Shouldly;

namespace Pot.App.Tests.Features.Accruals.Status;

public class AccrualsStatusServiceFixture
{
    [Fact]
    public void Should_Throw_When_AccountAccrualRepository_Is_Null()
    {
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var incomeRepository = Substitute.For<IIncomeRepository>();
        var logger = new FakeLogger<AccrualsStatusService>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(null!, expenseRepository, incomeRepository, logger);
        });
    }

    [Fact]
    public void Should_Throw_When_ExpenseRepository_Is_Null()
    {
        var accountAccrualRepository = Substitute.For<IAccountAccrualRepository>();
        var incomeRepository = Substitute.For<IIncomeRepository>();
        var logger = new FakeLogger<AccrualsStatusService>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(accountAccrualRepository, null!, incomeRepository, logger);
        });
    }

    [Fact]
    public void Should_Throw_When_IncomeRepository_Is_Null()
    {
        var accountAccrualRepository = Substitute.For<IAccountAccrualRepository>();
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var logger = new FakeLogger<AccrualsStatusService>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(accountAccrualRepository, expenseRepository, null!, logger);
        });
    }

    [Fact]
    public void Should_Throw_When_Logger_Is_Null()
    {
        var accountAccrualRepository = Substitute.For<IAccountAccrualRepository>();
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var incomeRepository = Substitute.For<IIncomeRepository>();

        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new AccrualsStatusService(accountAccrualRepository, expenseRepository, incomeRepository, null!);
        });
    }

    [Fact]
    public async Task Should_Get_Account_Accrual_Status_From_AccountAccrual_Repository()
    {
        var accountAccrualRepository = Substitute.For<IAccountAccrualRepository>();
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var incomeRepository = Substitute.For<IIncomeRepository>();

        var service = new AccrualsStatusService(
            accountAccrualRepository,
            expenseRepository,
            incomeRepository,
            new FakeLogger<AccrualsStatusService>());

        var accountRowIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var asOfDate = new DateOnly(2026, 4, 24);

        var expectedExpenseRenewals = new[] { accountRowIds[0] };
        var expectedAccountAccruals = new[] { accountRowIds[1] };
        var expectedIncomeRenewals = Array.Empty<Guid>();

        expenseRepository.GetRequiredRenewalsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>())
            .Returns(expectedExpenseRenewals);

        accountAccrualRepository.GetRequiredAccountAccrualsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>())
            .Returns(expectedAccountAccruals);

        incomeRepository.GetRequiredRenewalsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>())
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
        result.Value.AccountAccrualsRequired.ShouldBe(expectedAccountAccruals);
        result.Value.IncomeRenewalsRequired.ShouldBe(expectedIncomeRenewals);

        await accountAccrualRepository.Received(1)
            .GetRequiredAccountAccrualsAsync(accountRowIds, asOfDate, Arg.Any<CancellationToken>());

        await expenseRepository.DidNotReceive()
            .GetRequiredAccountAccrualsAsync(Arg.Any<Guid[]>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }
}
