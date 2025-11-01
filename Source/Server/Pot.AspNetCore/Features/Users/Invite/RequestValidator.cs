using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Users.Invite;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Username).IsNotEmpty();
        RuleFor(request => request.Email).IsNotEmpty();
        RuleFor(request => request.RoleIds).IsNotEmpty();
    }
}
