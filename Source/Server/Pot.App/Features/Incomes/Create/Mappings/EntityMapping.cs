using Pot.App.Features.Incomes.Create.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Create.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this IncomeEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}