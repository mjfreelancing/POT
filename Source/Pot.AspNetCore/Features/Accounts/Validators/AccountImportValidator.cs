using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Validators;

internal sealed class AccountImportValidator : ValidatorBase<AccountImport>
{
    public AccountImportValidator()
    {
        RuleFor(account => account.Id).IsGreaterThan(0);
        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
    }
}
