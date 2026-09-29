using AllOverIt.Assertion;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Default implementation of <see cref="IExpenseRenewalCalculator"/>.
/// </summary>
/// <remarks>
/// The renewal rules live in the pure IExpenseRenewalFold; this entity-facing calculator adapts the expense graph
/// to it and writes the folded cursor back, because a schedule advance is a fact worth persisting.
/// </remarks>
internal sealed class ExpenseRenewalCalculator : IExpenseRenewalCalculator
{
    private readonly IExpenseRenewalFold _renewalFold;

    public ExpenseRenewalCalculator(IExpenseRenewalFold renewalFold)
    {
        _renewalFold = renewalFold.WhenNotNull();
    }

    /// <inheritdoc />
    public void Renew(IEnumerable<ExpenseEntity> expenses, RenewalMode mode, DateOnly asOfDate)
    {
        _ = expenses.WhenNotNull();

        // asOfDate is typically 'today' (except when calculating projections)

        foreach (var expense in expenses)
        {
            var facts = expense.MapToAccrualInput();
            var cursor = new AccrualCursor(expense.NextDue, expense.AccrualStart);

            var renewed = _renewalFold.Renew(facts, cursor, mode, asOfDate);

            // Assigning an unchanged value is a no-op for a tracked entity, so a row the fold did not advance
            // keeps the values it already had.
            expense.NextDue = renewed.NextDue;
            expense.AccrualStart = renewed.AccrualStart;
        }
    }
}
