using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Pagination;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Expenses.GetAll.Mappings;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;

namespace Pot.App.Features.Expenses.GetAll;

internal sealed class GetExpensesService : IGetExpensesService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public GetExpensesService(IExpenseRepository expenseRepository, ILogger<GetExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<List<Output>> GetAllExpensesAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expenses = await _expenseRepository
            .GetAllExpensesAsync(cancellationToken)
            .ConfigureAwait(false);

        return expenses.SelectToList(expense => expense.MapToOutput());
    }

    public async Task<PageResult<Output>> GetAllExpensesAsync(Paging paging, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expenses = await _expenseRepository
            .GetAllExpensesPagedAsync(paging, cancellationToken)
            .ConfigureAwait(false);

        var outputs = expenses.Results.Select(result => result.MapToOutput());

        return PageResult<Output>.CreateFrom(expenses, outputs);
    }
}
