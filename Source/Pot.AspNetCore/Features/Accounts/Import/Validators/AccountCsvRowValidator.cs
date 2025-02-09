using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Models;

namespace Pot.AspNetCore.Features.Accounts.Import.Validators;

internal sealed class AccountCsvRowValidator : ValidatorBase<AccountCsvRow>, IAccountCsvRowValidator
{
    public AccountCsvRowValidator()
    {
        // Can be null but cannot be 00000000-0000-0000-0000-000000000000
        RuleFor(csvRow => csvRow.Id).IsNullOrNonDefault();
        RuleFor(csvRow => csvRow.Bsb).IsNotEmpty();
        RuleFor(csvRow => csvRow.Number).IsNotEmpty();
        RuleFor(csvRow => csvRow.Description).IsNotEmpty();
        RuleFor(csvRow => csvRow.Balance).IsGreaterThanOrEqualTo(0.0d);
        RuleFor(csvRow => csvRow.Reserved).IsGreaterThanOrEqualTo(0.0d);
    }
}
