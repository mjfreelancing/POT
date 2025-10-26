using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Auth.Signup.Send;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Username).IsNotEmpty();
        RuleFor(request => request.Email).IsNotEmpty();
    }
}
