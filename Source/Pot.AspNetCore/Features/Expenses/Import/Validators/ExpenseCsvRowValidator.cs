using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

internal sealed class ExpenseCsvRowValidator : ValidatorBase<ExpenseCsvRow>, IExpenseCsvRowValidator
{
    public ExpenseCsvRowValidator()
    {
        RuleFor(expense => expense.AccountId).IsNotEmpty(); // TODO: Can be null, but cannot be default
        RuleFor(expense => expense.ExpenseId).IsNotEmpty(); // TODO: Can be null, but cannot be default
        RuleFor(expense => expense.Description).IsNotEmpty();
        RuleFor(expense => expense.FrequencyCount).IsGreaterThan(0);
        RuleFor(expense => expense.Amount).IsGreaterThanOrEqualTo(0.0d);

        // TODO: Needs to be inclusive between 0 and Amount
        RuleFor(expense => expense.Allocated).IsGreaterThanOrEqualTo(0.0d);          // RuleFor(account => account.Allocated).IsInclusiveBetween(0.0d, ...);
    }
}
