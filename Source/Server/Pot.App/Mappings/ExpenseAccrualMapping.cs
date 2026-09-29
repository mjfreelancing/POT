using Pot.Data.Entities;
using Pot.Shared.Models;

namespace Pot.App.Mappings;

/// <summary>
/// Projects a loaded expense entity into the accrual calculation's input model.
/// </summary>
/// <remarks>
/// The read paths load the whole expense entity because the response payload needs it, so the accrual facts are
/// projected in memory here. The accounts query projects the same facts in the database instead, because it never
/// needs the expense graph.
/// </remarks>
internal static class ExpenseAccrualMapping
{
    /// <summary>
    /// Projects the accrual-relevant facts from the expense.
    /// </summary>
    /// <param name="expense">The expense to project.</param>
    /// <returns>The immutable facts the calculation consumes.</returns>
    public static ExpenseAccrualInput MapToAccrualInput(this ExpenseEntity expense)
    {
        return new ExpenseAccrualInput
        {
            RowId = expense.RowId,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            AccrualStart = expense.AccrualStart,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount
        };
    }
}
