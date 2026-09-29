using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Folds an expense's schedule cursor forward without mutating the expense.
/// </summary>
/// <remarks>
/// The projection loop walks days against immutable facts, so renewal has to return the advanced cursor rather
/// than write it back to an entity. The command paths use the entity-facing calculator, which persists the same
/// fold's result.
/// </remarks>
public interface IExpenseRenewalFold : IPotSingletonDependency
{
    /// <summary>
    /// Renews the expense by folding its cursor.
    /// </summary>
    /// <param name="facts">The expense's immutable schedule facts.</param>
    /// <param name="cursor">The schedule position to fold from.</param>
    /// <param name="mode">The renewal mode.</param>
    /// <param name="asOfDate">The date renewal is evaluated for.</param>
    /// <returns>
    /// The folded cursor, or <paramref name="cursor"/> unchanged when the expense does not renew.
    /// </returns>
    AccrualCursor Renew(ExpenseAccrualInput facts, AccrualCursor cursor, RenewalMode mode, DateOnly asOfDate);
}
