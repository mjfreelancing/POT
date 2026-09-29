using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.App.Features.Expenses.GetAll.Mappings;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Shared.Models;

namespace Pot.App.Features.Expenses.GetAll;

/// <summary>
/// Default implementation of <see cref="IGetExpensesService"/>.
/// </summary>
internal sealed class GetExpensesService : IGetExpensesService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAccrualCalculator _accrualCalculator;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public GetExpensesService(IExpenseRepository expenseRepository, IAccrualCalculator accrualCalculator,
        ITimeProvider timeProvider, ILogger<GetExpensesService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _accrualCalculator = accrualCalculator.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<Output[]> GetAllExpensesAsync(CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expenses = await _expenseRepository
            .GetAllExpensesAsync(cancellationToken)
            .ConfigureAwait(false);

        var accruals = CalculateAccruals(expenses);

        return expenses.SelectToArray(expense => expense.MapToOutput(accruals[expense.RowId]));
    }

    // Calculates the accrual detail for each supplied expense, keyed by expense RowId.
    //
    // The whole batch is measured against one as-of date, captured once, so every row in the response reflects the
    // same day even if the call runs across midnight.
    //
    // The rows are grouped by account because CalculateAccountWithExpenseDetail returns an account-scoped
    // AccountAccrualView: its totals aggregate only the positions supplied in the call, while its per-expense
    // results carry the RowId of the facts they were calculated from. Each account's expenses are therefore
    // supplied as one set, and the results are attributed back to their rows by identity to build the flat lookup
    // the caller indexes into.
    //
    // Each expense is positioned with AtPersistedSchedule, which seeds the schedule cursor from its own persisted
    // schedule. A read measures today, so the cursor is never folded forward here; only the projection loop
    // advances a schedule, because walking the forecast window moves it day by day.
    //
    // Expenses excluded from calculations need no special handling: the calculator returns a zeroed detail for
    // them, so every supplied expense has an entry in the result.
    private Dictionary<Guid, ExpenseAccrualDetail> CalculateAccruals(List<ExpenseEntity> expenses)
    {
        var asOfDate = _timeProvider.GetLocalDateNow();
        var accruals = new Dictionary<Guid, ExpenseAccrualDetail>();

        // The detail shape is produced per account, so the rows are grouped the same way
        // and each result is attributed back to its row by identity.
        foreach (var accountExpenses in expenses.GroupBy(expense => expense.Account.Id))
        {
            // Each position is a point on the expense's schedule.
            // AtPersistedSchedule pins the facts to where that schedule currently stands.
            var positions = accountExpenses
                .Select(expense =>
                {
                    var accrualFacts = expense.MapToAccrualInput();
                    return ExpenseAccrualPosition.AtPersistedSchedule(accrualFacts);
                });

            var accountAccruals = _accrualCalculator.CalculateAccountWithExpenseDetail(positions, asOfDate);

            foreach (var accrual in accountAccruals.Expenses)
            {
                accruals[accrual.RowId] = accrual;
            }
        }

        return accruals;
    }
}
