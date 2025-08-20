using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;

namespace Pot.AspNetCore.Features.Incomes.ToggleExclude;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.RowIds).IsNotEmpty();
    }
}