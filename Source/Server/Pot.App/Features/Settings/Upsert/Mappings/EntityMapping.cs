using Pot.App.Features.Settings.Upsert.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Settings.Upsert.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this SettingEntity entity)
    {
        return new Output
        {
            RowId = entity.RowId,
            Etag = entity.Etag
        };
    }
}
