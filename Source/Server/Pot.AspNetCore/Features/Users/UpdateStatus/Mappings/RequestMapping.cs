using Pot.App.Features.Users.UpdateStatus.Models;

namespace Pot.AspNetCore.Features.Users.UpdateStatus.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId)
    {
        return new Input
        {
            RowId = userId,
            Etag = request.Etag,
            Status = request.Status
        };
    }
}
