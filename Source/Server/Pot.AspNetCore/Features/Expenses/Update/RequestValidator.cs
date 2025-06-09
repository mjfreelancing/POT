using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(Expense => Expense.Description).IsNotEmpty();
        RuleFor(Expense => Expense.FrequencyCount).IsGreaterThanOrEqualTo(1);
        RuleFor(Expense => Expense.Amount).IsGreaterThanOrEqualTo(0.0d);
    }
}
