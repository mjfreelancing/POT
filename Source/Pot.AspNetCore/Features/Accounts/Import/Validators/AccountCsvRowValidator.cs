using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Validators;

internal sealed class AccountCsvRowValidator : ValidatorBase<AccountCsvRow>, IAccountCsvRowValidator
{
    public AccountCsvRowValidator()
    {
        RuleFor(expense => expense.Id).IsNotEmpty();    // TODO: Can be null, but cannot be default
        RuleFor(account => account.Bsb).IsNotEmpty();
        RuleFor(account => account.Number).IsNotEmpty();
        RuleFor(account => account.Description).IsNotEmpty();
        RuleFor(account => account.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(account => account.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
