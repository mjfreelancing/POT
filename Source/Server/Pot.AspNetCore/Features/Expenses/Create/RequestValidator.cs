using AllOverIt.Validation.Extensions;
using FluentValidation.Results;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Shared;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Description).IsNotEmpty();

        // It's more effort to use a custom property validator than simply use a custom rule.
        // Check out the implementation of AccrualStartValidator in the commented code at the bottom.
        // RuleFor(request => request.AccrualStart).SetValidator(new AccrualStartValidator());

        // Can be before/after the next due date, but not after the end date
        this.CustomRuleFor(request => request.AccrualStart, (value, context) =>
        {
            var validationContext = context.GetContextData<Request, RequestValidationContext>();

            if (validationContext.EndDate.HasValue)
            {
                if (value > validationContext.EndDate.Value)
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

// This would be the way to go if the validator was re-usable, but it's specific to each request type.
// For the NextDue/EndDate validation, We could create an interface and apply ISP (a type constraint on
// a generic model type), but it's not worth the effort / maintenance.
//
//internal sealed class AccrualStartValidator : PropertyValidator<Request, DateOnly>
//{
//    public override string Name => nameof(AccrualStartValidator);

//    public override bool IsValid(ValidationContext<Request> context, DateOnly value)
//    {
//        var validationContext = context.GetContextData<Request, RequestValidationContext>();

//        if (validationContext.EndDate.HasValue)
//        {
//            if (value > validationContext.EndDate.Value)
//            {
//                var errorCode = ErrorCodes.Invalid;

//                var failure = new ValidationFailure
//                {
//                    PropertyName = nameof(Request.AccrualStart),
//                    AttemptedValue = value,
//                    ErrorCode = errorCode,
//                    ErrorMessage = GetDefaultMessageTemplate(errorCode)
//                };

//                context.AddFailure(failure);

//                // Return true to prevent FluentValidation from adding its own error
//                // using GetDefaultMessageTemplate()
//                return true;
//            }
//        }

//        return true;
//    }

//    protected override string GetDefaultMessageTemplate(string errorCode)
//    {
//        return "Cannot be after the end date";
//    }
//}
