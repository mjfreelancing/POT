using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Repository
{
    // TODO: Change to use the same approach as IAccountRepository
    //       which will mean the registration of this interface can be removed
    public interface IExpenseImportRepository
    {
        Task ImportExpensesAsync(ExpenseImport[] expenses, CancellationToken cancellationToken);
    }
}
