using Pot.App.Features.Approvals.UpdateStatus.Models;

namespace Pot.AspNetCore.Features.Approvals.UpdateStatus;

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
