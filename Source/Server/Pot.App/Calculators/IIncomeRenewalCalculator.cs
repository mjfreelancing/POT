using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Calculators;

/// <summary>
/// Advances and persists the schedule of one or more incomes.
/// </summary>
/// <remarks>
/// Renewal is an entity-facing operation: each income's schedule is folded forward from the supplied mode and
/// as-of date, and the advanced schedule is written back to the tracked entity so it can be saved.
/// </remarks>
public interface IIncomeRenewalCalculator : IPotSingletonDependency
{
    /// <summary>
    /// Renews the supplied incomes.
    /// </summary>
    /// <param name="incomes">The incomes to renew. An income that does not renew is left unchanged.</param>
    /// <param name="mode">The renewal mode that decides how far the schedule advances.</param>
    /// <param name="asOfDate">
    /// The date renewal is evaluated for; typically today, except when calculating projections.
    /// </param>
    void Renew(IEnumerable<IncomeEntity> incomes, RenewalMode mode, DateOnly asOfDate);
}
