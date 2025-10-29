using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Users.UpdateRoles.Mappings;

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