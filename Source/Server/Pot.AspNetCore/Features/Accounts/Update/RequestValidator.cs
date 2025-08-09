using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    static RequestValidator()
    {
        // Prevent RowId from being split into two words
        DisablePropertyNameSplitting();
    }

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
