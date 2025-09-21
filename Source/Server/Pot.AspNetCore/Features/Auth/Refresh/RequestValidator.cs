using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Auth.Refresh;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.RefreshToken).IsNotEmpty();
    }
}
