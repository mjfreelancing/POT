using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Users.Update;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.DisplayName).IsNotEmpty();
        RuleFor(request => request.Email).IsNotEmpty();
    }
}
