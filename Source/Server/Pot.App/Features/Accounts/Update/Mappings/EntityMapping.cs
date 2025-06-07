using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Accounts.Update.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}