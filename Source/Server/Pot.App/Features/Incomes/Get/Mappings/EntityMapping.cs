using Pot.App.Features.Incomes.Get.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Get.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this IncomeEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag,
            Description = entity.Description,
            NextDue = entity.NextDue,
            EndDate = entity.EndDate,
            Frequency = entity.Frequency,
            FrequencyCount = entity.FrequencyCount,
            Amount = entity.Amount,
            Note = entity.Note,
            Account = new Output.AccountModel
            {
                RowId = entity.Account.RowId,
                Description = entity.Account.Description
            }
        };
    }
}