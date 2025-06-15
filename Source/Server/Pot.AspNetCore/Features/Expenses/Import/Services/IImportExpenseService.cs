using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Features.Expenses.Import.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.Expenses.Import.Services;

public interface IImportExpenseService : IPotScopedDependency
{
    Task<EnrichedResult<ImportSummary>> ImportExpensesAsync(IEnumerable<ExpenseCsvRow> csvRows, CancellationToken cancellationToken);
}
