using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.RowId).IsNotEmpty();
        RuleFor(request => request.Bsb).IsNotEmpty();
        RuleFor(request => request.Number).IsNotEmpty();
        RuleFor(request => request.Description).IsNotEmpty();
        RuleFor(request => request.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(request => request.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
