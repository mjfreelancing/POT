using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Sites.Delete;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.SiteName).IsNotEmpty();
        RuleFor(request => request.Username).IsNotEmpty();
        RuleFor(request => request.Password).IsNotEmpty();
    }
}
