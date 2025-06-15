using AllOverIt.Assertion;
using AllOverIt.Async;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Projections;

internal sealed class ProjectionsService : IProjectionsService
{
    private readonly IExpenseRepositoryFactory _expenseRepositoryFactory;
    private readonly IIncomeRepositoryFactory _incomeRepositoryFactory;
    private readonly ILogger _logger;

    public ProjectionsService(IExpenseRepositoryFactory expenseRepositoryFactory, IIncomeRepositoryFactory incomeRepositoryFactory, ILogger<ProjectionsService> logger)
    {
        _expenseRepositoryFactory = expenseRepositoryFactory.WhenNotNull();
        _incomeRepositoryFactory = incomeRepositoryFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task GetProjectionsAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Can't execute two queries in parallel using the same DbContext.
        var _expenseRepository = _expenseRepositoryFactory.CreateExpenseRepository();
        var _incomeRepository = _incomeRepositoryFactory.CreateIncomeRepository();

        var (expenses, incomes) = await TaskHelper.WhenAll(
            _expenseRepository.GetAllExpensesAsync(cancellationToken),
            _incomeRepository.GetAllIncomesAsync(cancellationToken)
        );


    }
}

