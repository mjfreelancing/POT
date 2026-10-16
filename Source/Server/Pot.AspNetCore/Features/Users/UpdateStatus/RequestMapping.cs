using Pot.App.Features.Users.UpdateStatus.Models;

namespace Pot.AspNetCore.Features.Users.UpdateStatus;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId, IReadOnlyList<Guid> platformAdminRowIds)
    {
        return new Input
        {
            RowId = userId,
            Etag = request.Etag,
            Status = request.Status,
            PlatformAdminRowIds = platformAdminRowIds
        };
    }
}
