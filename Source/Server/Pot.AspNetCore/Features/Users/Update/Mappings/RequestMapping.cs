using Pot.App.Features.Users.Update.Models;

namespace Pot.AspNetCore.Features.Users.Update.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, Guid userId)
    {
        return new Input
        {
            RowId = userId,
            Etag = request.Etag,
            DisplayName = request.DisplayName,
            Email = request.Email
        };
    }
}
