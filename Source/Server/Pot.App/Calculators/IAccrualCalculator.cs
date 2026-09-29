using Pot.Shared.DependencyInjection;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Calculates expense accrual from explicit facts and a schedule cursor, for a given as-of date.
/// </summary>
/// <remarks>
/// The calculation is a pure function of its arguments: nothing is mutated and no derived value can be written
/// back to the database. Callers supply the schedule cursor explicitly, so the projection loop can fold the
/// schedule forward day by day while the calculation always measures an immutable fact set.
/// </remarks>
public interface IAccrualCalculator : IPotScopedDependency
{
    /// <summary>
    /// Calculates an account's accrual aggregates without maintaining per-expense detail.
    /// </summary>
    /// <param name="positions">The accrual facts and measurement cursors for a single account.</param>
    /// <param name="asOfDate">The date the accrual is evaluated for.</param>
    /// <returns>An <see cref="AccountAccrualView"/> whose <see cref="AccountAccrualView.Expenses"/> is empty.</returns>
    AccountAccrualView CalculateAccountTotals(IEnumerable<ExpenseAccrualPosition> positions, DateOnly asOfDate);

    /// <summary>
    /// Calculates an account's accrual aggregates together with a result for every expense.
    /// </summary>
    /// <param name="positions">The accrual facts and measurement cursors for a single account.</param>
    /// <param name="asOfDate">The date the accrual is evaluated for.</param>
    /// <returns>
    /// An <see cref="AccountAccrualView"/> whose <see cref="AccountAccrualView.Expenses"/> holds one entry per
    /// supplied position, in the order the positions were supplied.
    /// </returns>
    AccountAccrualView CalculateAccountWithExpenseDetail(IEnumerable<ExpenseAccrualPosition> positions, DateOnly asOfDate);
}
