using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Calculators;

/// <summary>
/// Advances and persists the schedule of one or more expenses.
/// </summary>
/// <remarks>
/// Renewal is an entity-facing operation: each expense's schedule is folded forward from the supplied mode and
/// as-of date, and the advanced cursor is written back to the tracked entity so it can be saved.
/// </remarks>
public interface IExpenseRenewalCalculator : IPotSingletonDependency
{
    /// <summary>
    /// Renews the supplied expenses.
    /// </summary>
    /// <param name="expenses">The expenses to renew. An expense that does not renew is left unchanged.</param>
    /// <param name="mode">The renewal mode that decides how far the schedule advances.</param>
    /// <param name="asOfDate">
    /// The date renewal is evaluated for; typically today, except when calculating projections.
    /// </param>
    void Renew(IEnumerable<ExpenseEntity> expenses, RenewalMode mode, DateOnly asOfDate);
}
