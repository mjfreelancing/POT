using AllOverIt.Extensions;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

public interface IExpenseImportValidator : IValidator<ExpenseForImport> //: IPotScopedDependency
{
}

internal sealed class ExpenseImportValidator : ValidatorBase<ExpenseForImport>, IExpenseImportValidator
{
    public ExpenseImportValidator()
    {
        RuleFor(expense => expense.AccountId)
            .Custom((accountId, context) =>
            {
                if (accountId.IsNotNullOrEmpty() && !Guid.TryParse(accountId, out var _))
                {
                    var failure = new ValidationFailure(
                        nameof(ExpenseForImport.AccountId),
                        $"The Id is invalid",
                        accountId)
                    {
                        ErrorCode = ErrorCodes.Invalid
                    };

                    context.AddFailure(failure);
                }
            });

        RuleFor(expense => expense.Description).IsNotEmpty();
        RuleFor(expense => expense.FrequencyCount).IsGreaterThan(0);
        RuleFor(expense => expense.Amount).IsGreaterThanOrEqualTo(0.0d);    // May be importing as a marker

        // TODO: Needs to be inclusive between 0 and Amount
        RuleFor(expense => expense.Allocated).IsGreaterThanOrEqualTo(0.0d);          // RuleFor(account => account.Allocated).IsInclusiveBetween(0.0d, ...);
    }
}
