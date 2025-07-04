using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Delete;

internal sealed class DeleteExpenseService : IDeleteExpenseService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public DeleteExpenseService(IPersistableExpenseRepository expenseRepository, ILogger<DeleteExpenseService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> DeleteExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expense = await _expenseRepository.GetExpenseOrDefaultAsync(expenseId, cancellationToken);

        if (expense is null)
        {
            var expenseIdNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(expenseId, "The expense does not exist.");

            _logger.LogError(expenseIdNotFoundDetails);

            return EnrichedResult.Fail<bool>(expenseIdNotFoundDetails);
        }

        _expenseRepository.Delete(expense);

        await _expenseRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(true);
    }
}
