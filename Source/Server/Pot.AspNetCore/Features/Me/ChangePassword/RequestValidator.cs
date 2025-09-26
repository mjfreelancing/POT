using AllOverIt.Validation.Extensions;
using FluentValidation;
using Pot.AspNetCore.Concerns.Validation;

namespace Pot.AspNetCore.Features.Me.ChangePassword;

internal sealed class RequestValidator : PotValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.CurrentPassword).IsNotEmpty();
        RuleFor(request => request.NewPassword).IsNotEmpty();

        RuleFor(request => request)
            .Must(request => request.CurrentPassword != request.NewPassword)
            .WithMessage("The new password must be different from the current password.");
    }
}
