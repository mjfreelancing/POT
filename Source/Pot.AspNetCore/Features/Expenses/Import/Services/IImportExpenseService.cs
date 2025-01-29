using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.AspNetCore.Features.Expenses.Import.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Services;

public interface IImportExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<ImportSummary>> ImportExpensesAsync(IEnumerable<ExpenseForImport> expensesForImport, bool overwrite, CancellationToken cancellationToken);
}
