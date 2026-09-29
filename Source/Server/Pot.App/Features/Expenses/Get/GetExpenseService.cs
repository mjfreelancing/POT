using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Expenses.Get.Mappings;
using Pot.App.Features.Expenses.Get.Models;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Models;

namespace Pot.App.Features.Expenses.Get;

/// <summary>
/// Default implementation of <see cref="IGetExpenseService"/>.
/// </summary>
internal sealed class GetExpenseService : IGetExpenseService
{
    private readonly IPersistableExpenseRepository _expenseRepository;
    private readonly IAccrualCalculator _accrualCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public GetExpenseService(IPersistableExpenseRepository expenseRepository, IAccrualCalculator accrualCalculator,
        ITimeProvider timeProvider, ILogger<GetExpenseService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _accrualCalculator = accrualCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<Output>> GetExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expense = await _expenseRepository.GetExpenseOrDefaultAsync(expenseId, cancellationToken);

        if (expense is null)
        {
            var expenseNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(expenseId, "The expense does not exist");

            _logger.LogApiError(expenseNotFoundError);

            return EnrichedResult.Fail<Output>(expenseNotFoundError);
        }

        var accrualDetail = CalculateAccrual(expense);
        var output = expense.MapToOutput(accrualDetail);

        return EnrichedResult.Success(output);
    }

    private ExpenseAccrualDetail CalculateAccrual(ExpenseEntity expense)
    {
        var position = ExpenseAccrualPosition.AtPersistedSchedule(expense.MapToAccrualInput());

        var view = _accrualCalculator.CalculateAccountWithExpenseDetail([position], _timeProvider.GetLocalDateNow());

        // The detail shape holds one entry per position, in the order the positions were supplied.
        return view.Expenses[0];
    }
}
