using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.GetAll.Mappings;

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
            Account = new Output.AccountModel
            {
                RowId = entity.Account.RowId,
                Description = entity.Account.Description
            }
        };
    }
}