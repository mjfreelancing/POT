using Pot.App.Features.Me.Delete.Models;

namespace Pot.AspNetCore.Features.Me.Delete;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, IReadOnlyList<Guid> platformAdminRowIds)
    {
        return new Input
        {
            Username = request.Username,
            Password = request.Password,
            PlatformAdminRowIds = platformAdminRowIds
        };
    }
}
