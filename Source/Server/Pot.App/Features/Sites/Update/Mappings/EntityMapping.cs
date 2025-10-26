using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Sites.Update.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this SiteEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}