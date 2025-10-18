using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Username).IsNotEmpty();
        RuleFor(request => request.ReferenceCode).IsNotEmpty();
        RuleFor(request => request.VerificationCode).IsNotEmpty();
    }
}
