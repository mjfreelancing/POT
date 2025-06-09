using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Expenses.Get.Mappings;
using Pot.App.Features.Expenses.Get.Models;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Get;

internal sealed class GetExpenseService : IGetExpenseService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public GetExpenseService(IPersistableExpenseRepository expenseRepository, ILogger<GetExpenseService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<Output?> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expense = await _expenseRepository.GetExpenseOrDefaultAsync(expenseId, cancellationToken);

        return expense?.MapToOutput();
    }
}
