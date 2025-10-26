using Pot.App.Features.Auth.Signup.Request.Models;

namespace Pot.AspNetCore.Features.Auth.Signup.Send.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request, string correlationId)
    {
        return new Input
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            CorrelationId = correlationId
        };
    }
}
