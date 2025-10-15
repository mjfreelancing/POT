using Pot.App.Features.Auth.PasswordReset.Request.Models;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, string correlationId)
    {
        return new Input
        {
            Username = request.Username,
            CorrelationId = correlationId
        };
    }
}
