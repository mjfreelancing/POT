using Pot.App.Features.Auth.Signup.Complete.Models;

namespace Pot.AspNetCore.Features.Auth.Signup.Complete.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Username = request.Username.Trim(),
            ReferenceCode = request.ReferenceCode,
            VerificationCode = request.VerificationCode
        };
    }
}
