using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Folds an income's schedule forward without mutating the income.
/// </summary>
/// <remarks>
/// The projection loop walks days against immutable facts, so renewal has to return the advanced schedule rather
/// than write it back to an entity. The command paths use the entity-facing calculator, which persists the same
/// fold's result.
/// </remarks>
public interface IIncomeRenewalFold : IPotSingletonDependency
{
    /// <summary>
    /// Renews the income by folding its schedule.
    /// </summary>
    /// <param name="schedule">The income's immutable schedule facts.</param>
    /// <param name="mode">The renewal mode.</param>
    /// <param name="asOfDate">The date renewal is evaluated for.</param>
    /// <returns>
    /// The folded schedule, or <paramref name="schedule"/> unchanged when the income does not renew.
    /// </returns>
    IncomeScheduleInput Renew(IncomeScheduleInput schedule, RenewalMode mode, DateOnly asOfDate);
}
