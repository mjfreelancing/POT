using Pot.App.Features.Users.UpdateRoles.Models;

namespace Pot.AspNetCore.Features.Users.UpdateRoles;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId)
    {
        return new Input
        {
            RowId = userId,
            Etag = request.Etag,
            RoleIds = request.RoleIds
        };
    }
}
