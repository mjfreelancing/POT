using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Pot.App.Errors;

namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        this.CustomRuleFor(request => request.StartDate, (value, context) =>
        {
            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (value < validationContext.Today)
            {
                var failure = new ValidationFailure(nameof(Request.StartDate), "Cannot be earlier than today", value)
                {
                    ErrorCode = ErrorCodes.Invalid
                };

                context.AddFailure(failure);
            }
        });

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("The start date must be less than or equal to the end date.");
    }
}
