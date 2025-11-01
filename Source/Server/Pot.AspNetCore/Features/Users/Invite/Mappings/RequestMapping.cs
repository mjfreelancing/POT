using Pot.App.Features.Users.Invite.Models;

namespace Pot.AspNetCore.Features.Users.Invite.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            RoleIds = request.RoleIds
        };
    }
}
