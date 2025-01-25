using AllOverIt.Extensions;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class RequestValidator : ValidatorBase<Request>
{
    public RequestValidator()
    {
        RuleFor(account => account.RowId).Custom((id, context) =>
        {
            if (id.IsNullOrEmpty() || !Guid.TryParse(id, out var accountId))
            {
                var failure = new ValidationFailure(
                    nameof(AccountEntity.RowId),
                    $"The account Id is invalid",
                    id ?? string.Empty)
                {
                    ErrorCode = ErrorCodes.Invalid
                };

                context.AddFailure(failure);
            }
        });

        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Number).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
