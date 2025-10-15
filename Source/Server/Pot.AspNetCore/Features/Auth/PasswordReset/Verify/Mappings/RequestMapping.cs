using Pot.App.Features.Auth.PasswordReset.Verify.Models;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify.Mappings;

internal static class RequestMapping
{
    public static Input MapToInput(this Request request)
    {
        return new Input
        {
            Username = request.Username,
            ReferenceCode = request.ReferenceCode,
            VerificationCode = request.VerificationCode
        };
    }
}
