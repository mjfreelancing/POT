using AllOverIt.Validation;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Validation.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Validators;

internal sealed class AccountImportArrayValidator : ValidatorBase<AccountImport[]>
{
    public AccountImportArrayValidator()
    {
        RuleFor(accounts => accounts).IsUnique(account => account.Id);
        RuleFor(accounts => accounts).IsUnique(account => account.Description);

        // EXTEND THE IsUnique() so multiple columns can be provided => need to be able to identify all columns

        RuleForEach(accounts => accounts).SetValidator(new AccountImportValidator());
    }
}
