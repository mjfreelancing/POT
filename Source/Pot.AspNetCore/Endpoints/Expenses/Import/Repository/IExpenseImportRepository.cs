using Pot.AspNetCore.Endpoints.Expenses.Import.Models;

namespace Pot.AspNetCore.Endpoints.Expenses.Import.Repository
{
    public interface IExpenseImportRepository
    {
        Task ImportExpensesAsync(ExpenseImport[] expenses, CancellationToken cancellationToken);
    }
}
