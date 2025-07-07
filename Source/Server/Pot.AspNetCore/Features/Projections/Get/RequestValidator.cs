using AllOverIt.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("The start date must be less than or equal to the end date.");
    }
}
