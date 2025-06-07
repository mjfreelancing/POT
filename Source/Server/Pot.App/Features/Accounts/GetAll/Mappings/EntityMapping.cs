using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag,
            Bsb = entity.Bsb,
            Number = entity.Number,
            Description = entity.Description,
            Balance = entity.Balance,
            Reserved = entity.Reserved,
            Allocated = entity.Allocated,
            DailyAccrual = entity.DailyAccrual
        };
    }
}