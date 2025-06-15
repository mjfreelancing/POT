using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create.EntityChecks;

public interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(ExpenseEntity expenseToCreate, CancellationToken cancellationToken);
}
