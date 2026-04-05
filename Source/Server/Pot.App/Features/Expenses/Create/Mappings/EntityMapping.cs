using Pot.App.Features.Expenses.Create.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Create.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this ExpenseEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag,
            AccrualStart = entity.AccrualStart
        };
    }
}