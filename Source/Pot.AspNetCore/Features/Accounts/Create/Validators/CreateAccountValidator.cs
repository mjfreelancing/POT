using AllOverIt.Assertion;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation.Results;
using Pot.AspNetCore.Validation;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Validators;

// Adding IScopedLifetimeValidator since we are injecting an IAccountRepository
internal sealed class CreateAccountValidator : ValidatorBase<AccountEntity>, IScopedLifetimeValidator
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();

        RuleForAccountDoesNotExist();
    }

    private void RuleForAccountDoesNotExist()
    {
        this.CustomRuleForAsync(
            entity => entity,
            async (entity, context, cancellationToken) =>
            {
                var accountExists = await _accountRepository.AccountExistsAsync(entity.Bsb, entity.Number, cancellationToken);

                if (accountExists)
                {
                    var failure = new ValidationFailure(
                        $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                        $"An account already exists with the provided {nameof(entity.Bsb)} and {nameof(entity.Number)}",
                        new { entity.Bsb, entity.Number })
                    {
                        ErrorCode = ErrorCodes.Conflict
                    };

                    context.AddFailure(failure);
                }
            });
    }
}
