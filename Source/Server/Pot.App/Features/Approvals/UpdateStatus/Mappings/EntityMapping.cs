using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Approvals.UpdateStatus.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this UserEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}