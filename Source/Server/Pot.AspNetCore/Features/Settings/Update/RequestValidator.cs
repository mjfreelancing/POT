using AllOverIt.Extensions;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Features.Settings.Update;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Value).NotNull();

        RuleFor(request => request)
            .Custom((_, context) =>
            {
                var validationContext = context.GetContextData<Request, RequestValidationContext>();

                if (!SettingCategory.TryFromName(validationContext.Category, out var _))
                {
                    context.AddFailure(nameof(validationContext.Category), $"'{validationContext.Category}' is not a valid setting category.");
                }
            });

        RuleFor(request => request)
            .Custom((_, context) =>
            {
                var validationContext = context.GetContextData<Request, RequestValidationContext>();

                if (validationContext.Key.IsNullOrEmpty())
                {
                    context.AddFailure(nameof(validationContext.Key), "Setting key cannot be empty.");
                }
            });
    }
}
