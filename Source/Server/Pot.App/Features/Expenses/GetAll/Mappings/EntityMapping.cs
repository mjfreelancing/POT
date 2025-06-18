using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this ExpenseEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag,
            Description = entity.Description,
            NextDue = entity.NextDue,
            EndDate = entity.EndDate,
            AccrualStart = entity.AccrualStart,
            Frequency = entity.Frequency,
            FrequencyCount = entity.FrequencyCount,
            Recurring = entity.EndDate == null,
            Amount = entity.Amount,
            Accrued = entity.Accrued,
            Account = new Output.AccountModel
            {
                RowId = entity.Account.RowId,
                Description = entity.Account.Description
            }
        };
    }
}