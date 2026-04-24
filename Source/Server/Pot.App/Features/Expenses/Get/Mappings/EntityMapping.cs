using Pot.App.Features.Expenses.Get.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Get.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this ExpenseEntity expense)
    {
        return new Output
        {
            RowId = expense.RowId,
            Etag = expense.Etag,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            Description = expense.Description,
            NextDue = expense.NextDue,
            AccrualStart = expense.AccrualStart,
            EndDate = expense.EndDate,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount,
            Accrued = expense.Accrued,
            Note = expense.Note,
            Account = new Output.AccountModel
            {
                RowId = expense.Account.RowId,
                Description = expense.Account.Description
            }
        };
    }
}