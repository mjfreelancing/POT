using AllOverIt.Validation.Extensions;
using FluentValidation.Results;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.ExcludeFromCalcs).IsRequired();
        RuleFor(request => request.Description).IsNotEmpty();

        // Can be before/after the next due date, but not after the end date.
        // Validate only when an accrual start date is provided.
        this.CustomRuleFor(request => request.AccrualStart, (value, context) =>
        {
            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (validationContext.AccrualPolicy == AccrualPolicy.None)
            {
                if (value.HasValue)
                {
                    var failure = new ValidationFailure(nameof(Request.AccrualStart), $"Must be empty when Accrual Policy is {AccrualPolicy.None.Name}", value)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }

                return;
            }

            if (!value.HasValue)
            {
                return;
            }

            if (validationContext.EndDate.HasValue)
            {
                if (value.Value > validationContext.EndDate.Value)
                {
                    var failure = new ValidationFailure(nameof(Request.AccrualStart), "Cannot be after the end date", value)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }
            }
        });

        this.CustomRuleFor(request => request.EndDate, (value, context) =>
        {
            if (!value.HasValue)
            {
                return;
            }

            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (validationContext.NextDue > value.Value)
            {
                var failure = new ValidationFailure(nameof(Request.EndDate), "Cannot be earlier than the next due date", value)
                {
                    ErrorCode = ErrorCodes.Invalid
                };

                context.AddFailure(failure);
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
        RuleFor(expense => expense.AccountRowId).IsNotEmpty();
    }
}
