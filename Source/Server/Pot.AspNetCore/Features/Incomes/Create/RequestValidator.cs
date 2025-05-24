using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Incomes.Create;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(income => income.Description).IsNotEmpty();
        RuleFor(income => income.FrequencyCount).IsGreaterThanOrEqualTo(1);
        RuleFor(income => income.Amount).IsGreaterThanOrEqualTo(0.0d);
    }
}
