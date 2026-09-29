using Pot.Data.Entities;
using Pot.Shared.Models;

namespace Pot.App.Mappings;

/// <summary>
/// Projects a loaded income entity into the renewal fold's input model.
/// </summary>
/// <remarks>
/// The renewal paths hold the income graph, so the schedule facts are projected in memory here. An income has no
/// accrual state, so the income side of the fold carries only schedule facts.
/// </remarks>
internal static class IncomeScheduleMapping
{
    /// <summary>
    /// Projects the schedule facts from the income.
    /// </summary>
    /// <param name="income">The income to project.</param>
    /// <returns>The immutable schedule the renewal fold consumes.</returns>
    public static IncomeScheduleInput MapToScheduleInput(this IncomeEntity income)
    {
        return new IncomeScheduleInput
        {
            RowId = income.RowId,
            NextDue = income.NextDue,
            EndDate = income.EndDate,
            Frequency = income.Frequency,
            FrequencyCount = income.FrequencyCount,
            ExcludeFromCalcs = income.ExcludeFromCalcs
        };
    }
}
