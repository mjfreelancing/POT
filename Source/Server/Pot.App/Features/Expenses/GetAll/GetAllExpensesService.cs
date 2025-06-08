using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Pagination;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Expenses.GetAll.Mappings;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Shared;

namespace Pot.App.Features.Expenses.GetAll;

internal sealed class GetAllExpensesService : IGetAllExpensesService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public GetAllExpensesService(IExpenseRepository expenseRepository, ILogger<GetAllExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<PageResult<Output>> GetAllExpensesAsync(Paging paging, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var incomes = await _expenseRepository.GetAllExpensesAsync(paging, cancellationToken);

        var outputs = incomes.Results.Select(result => result.MapToOutput());

        return PageResult<Output>.CreateFrom(incomes, outputs);
    }
}
