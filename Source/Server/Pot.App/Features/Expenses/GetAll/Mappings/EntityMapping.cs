using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Data.Entities;
using Pot.Shared.Models;

namespace Pot.App.Features.Expenses.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this ExpenseEntity expense, ExpenseAccrualDetail accrual)
    {
        return new Output
        {
            RowId = expense.RowId,
            Etag = expense.Etag,
            ExcludeFromCalcs = expense.ExcludeFromCalcs,
            Description = expense.Description,
            NextDue = expense.NextDue,
            EndDate = expense.EndDate,
            AccrualStart = expense.AccrualStart,
            AccrualPolicy = expense.AccrualPolicy,
            Frequency = expense.Frequency,
            FrequencyCount = expense.FrequencyCount,
            Amount = expense.Amount,
            Accrued = accrual.Accrued,
            Arrears = accrual.Arrears,
            Note = expense.Note,
            Account = new Output.AccountModel
            {
                RowId = expense.Account.RowId,
                Description = expense.Account.Description
            }
        };
    }
}