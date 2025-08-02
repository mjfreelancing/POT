using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation.Results;
using Pot.App.Errors;
using Pot.Shared;

namespace Pot.AspNetCore.Features.Incomes.Create;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Description).IsNotEmpty();

        this.CustomRuleFor(request => request.EndDate, (value, context) =>
        {
            if (value.HasValue)
            {
                var validationContext = context.GetContextData<Request, RequestValidationContext>();

                if (validationContext.NextDue > value.Value)
                {
                    var failure = new ValidationFailure(nameof(Request.EndDate), "Cannot be earlier than the next due date", value)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }
            }
        });

        this.CustomRuleFor(request => request.FrequencyCount, (value, context) =>
        {
            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (validationContext.Frequency == Frequency.OneTime)
            {
                if (value != 0)
                {
                    var failure = new ValidationFailure(nameof(Request.FrequencyCount), $"Must be zero when Frequency is {Frequency.OneTime.Name}", value)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }
            }
            else
            {
                if (value < 1)
                {
                    var failure = new ValidationFailure(nameof(Request.FrequencyCount), "Must be greater than zero", value)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }
            }
        });

        RuleFor(request => request.Amount).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(request => request.AccountRowId).IsNotEmpty();
    }
}
