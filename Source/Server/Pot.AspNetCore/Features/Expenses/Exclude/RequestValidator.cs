using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Expenses.Exclude;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.RowIds).IsNotEmpty();
        RuleFor(request => request.ExcludeFromCalcs).IsNotEmpty();
    }
}