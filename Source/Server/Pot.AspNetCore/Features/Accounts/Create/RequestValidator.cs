using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Number).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
