using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(expense => expense.Description).IsNotEmpty();
        RuleFor(expense => expense.FrequencyCount).IsGreaterThanOrEqualTo(1);
        RuleFor(expense => expense.Amount).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(expense => expense.AccountRowId).IsNotEmpty();
    }
}
