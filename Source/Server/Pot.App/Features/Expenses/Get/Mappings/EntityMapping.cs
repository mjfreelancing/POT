using Pot.App.Features.Expenses.Get.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Get.Mappings;

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
            AccrualStart = entity.AccrualStart,
            EndDate = entity.EndDate,
            Frequency = entity.Frequency,
            FrequencyCount = entity.FrequencyCount,
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