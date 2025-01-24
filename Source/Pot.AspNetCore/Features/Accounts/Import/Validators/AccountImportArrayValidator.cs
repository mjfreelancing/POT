using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Validators;

internal sealed class AccountImportArrayValidator : ValidatorBase<AccountForImport[]>
{
    public AccountImportArrayValidator()
    {
        RuleFor(accounts => accounts).IsUnique(account => account.Description);
        RuleFor(accounts => accounts).IsUnique(account => account.Bsb, account => account.Number);

        RuleForEach(accounts => accounts).SetValidator(new AccountImportValidator());
    }
}
