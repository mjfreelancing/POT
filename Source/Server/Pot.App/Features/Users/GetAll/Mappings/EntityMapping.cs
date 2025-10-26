using Pot.App.Features.Users.GetAll.Models;
using Pot.Data.Repositories.Users.Dtos;

namespace Pot.App.Features.Users.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this GetAllUserInfo userInfo)
    {
        return new Output
        {
            RowId = userInfo.RowId,
            Etag = userInfo.Etag,
            Username = userInfo.Username,
            DisplayName = userInfo.DisplayName,
            Email = userInfo.Email,
            Roles = userInfo.Roles,
            LastLoggedInUtc = userInfo.LastLoggedInUtc
        };
    }
}