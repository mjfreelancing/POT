using Pot.App.Features.Roles.GetAll.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Roles.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this RoleEntity role)
    {
        return new Output
        {
            RowId = role.RowId,
            Etag = role.Etag,
            Name = role.Name.Name
        };
    }
}