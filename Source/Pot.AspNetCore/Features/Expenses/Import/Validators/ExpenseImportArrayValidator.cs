//using AllOverIt.Validation;
//using AllOverIt.Validation.Extensions;
//using Pot.AspNetCore.Features.Expenses.Import.Models;

//namespace Pot.AspNetCore.Features.Expenses.Import.Validators;

//internal sealed class ExpenseImportArrayValidator : ValidatorBase<ExpenseForImport[]>
//{
//    public ExpenseImportArrayValidator()
//    {
//        RuleFor(expenses => expenses).IsUnique(account => account.AccountId, account => account.Description);

//        RuleForEach(expenses => expenses).SetValidator(new ExpenseImportValidator());
//    }
//}
