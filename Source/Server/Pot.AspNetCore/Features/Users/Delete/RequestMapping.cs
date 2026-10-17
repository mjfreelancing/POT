using Pot.App.Features.Users.Delete.Models;

namespace Pot.AspNetCore.Features.Users.Delete;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId, IReadOnlyList<Guid> platformAdminRowIds)
    {
        return new Input
        {
            RowId = userId,
            Password = request.Password,
            PlatformAdminRowIds = platformAdminRowIds
        };
    }
}
