using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Validators;

internal sealed class AccountImportValidator : ValidatorBase<AccountForImport>
{
    public AccountImportValidator()
    {
        RuleFor(account => account.Id).IsGreaterThan(0);
        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
    }
}
