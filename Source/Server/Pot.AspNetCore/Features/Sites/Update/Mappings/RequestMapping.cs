using Pot.App.Features.Sites.Update.Models;

namespace Pot.AspNetCore.Features.Sites.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId)
    {
        return new Input
        {
            RowId = userId,
            Etag = request.Etag,
            Name = request.Name,
            Description = request.Description
        };
    }
}
