using Pot.App.Features.Auth.Me.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Auth.Me.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this UserEntity user)
    {
        return new Output
        {
            RowId = user.RowId,
            Etag = user.Etag,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email
        };
    }
}