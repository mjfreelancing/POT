using Pot.App.Features.Sites.Delete.Models;

namespace Pot.AspNetCore.Features.Sites.Delete;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, IReadOnlyList<Guid> platformAdminRowIds)
    {
        return new Input
        {
            SiteName = request.SiteName,
            Username = request.Username,
            Password = request.Password,
            PlatformAdminRowIds = platformAdminRowIds
        };
    }
}
