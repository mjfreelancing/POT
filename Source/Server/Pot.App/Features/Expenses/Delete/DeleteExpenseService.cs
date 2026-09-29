using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Expenses.Delete;

/// <summary>
/// Default implementation of <see cref="IDeleteExpenseService"/>.
/// </summary>
internal sealed class DeleteExpenseService : IDeleteExpenseService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public DeleteExpenseService(IPersistableExpenseRepository expenseRepository, ILogger<DeleteExpenseService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<bool>> DeleteExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_expenseRepository.WithTracking())
        {
            var expense = await _expenseRepository
                .GetExpenseOrDefaultAsync(expenseId, cancellationToken)
                .ConfigureAwait(false);

            if (expense is null)
            {
                var expenseNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(expenseId, "The expense does not exist");

                _logger.LogApiError(expenseNotFoundError);

                return EnrichedResult.Fail<bool>(expenseNotFoundError);
            }

            _expenseRepository.Delete(expense);

            await _expenseRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(true);
    }
}
