using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Repository
{
    public interface IExpenseImportRepository
    {
        Task ImportExpensesAsync(ExpenseImport[] expenses, CancellationToken cancellationToken);
    }
}
