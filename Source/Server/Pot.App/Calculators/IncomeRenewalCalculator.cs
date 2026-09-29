using AllOverIt.Assertion;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.App.Calculators;

/// <summary>
/// Default implementation of <see cref="IIncomeRenewalCalculator"/>.
/// </summary>
// The renewal rules live in the pure IIncomeRenewalFold; this entity-facing calculator adapts the income graph to
// it and writes the folded schedule back, because a schedule advance is a fact worth persisting.
internal sealed class IncomeRenewalCalculator : IIncomeRenewalCalculator
{
    private readonly IIncomeRenewalFold _renewalFold;

    public IncomeRenewalCalculator(IIncomeRenewalFold renewalFold)
    {
        _renewalFold = renewalFold.WhenNotNull();
    }

    /// <inheritdoc />
    // asOfDate is typically 'today' (except when calculating projections)
    public void Renew(IEnumerable<IncomeEntity> incomes, RenewalMode mode, DateOnly asOfDate)
    {
        _ = incomes.WhenNotNull();

        foreach (var income in incomes)
        {
            var schedule = income.MapToScheduleInput();

            var renewed = _renewalFold.Renew(schedule, mode, asOfDate);

            // Assigning an unchanged value is a no-op for a tracked entity, so a row the fold did not advance
            // keeps the values it already had.
            income.NextDue = renewed.NextDue;
        }
    }
}
