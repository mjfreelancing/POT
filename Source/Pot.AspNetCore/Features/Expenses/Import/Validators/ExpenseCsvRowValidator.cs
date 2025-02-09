using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

internal sealed class ExpenseCsvRowValidator : ValidatorBase<ExpenseCsvRow>, IExpenseCsvRowValidator
{
    public ExpenseCsvRowValidator()
    {
        // Can be null but cannot be 00000000-0000-0000-0000-000000000000
        RuleFor(csvRow => csvRow.AccountId).IsNullOrNonDefault();
        RuleFor(csvRow => csvRow.ExpenseId).IsNullOrNonDefault();
        RuleFor(csvRow => csvRow.Description).IsNotEmpty();
        RuleFor(csvRow => csvRow.FrequencyCount).IsGreaterThan(0);
        RuleFor(csvRow => csvRow.Amount).IsGreaterThanOrEqualTo(0.0d);

        // TODO: Needs to be inclusive between 0 and Amount
        RuleFor(expense => expense.Allocated).IsGreaterThanOrEqualTo(0.0d);          // RuleFor(account => account.Allocated).IsInclusiveBetween(0.0d, ...);
    }
}
