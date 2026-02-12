using Pot.App.Errors;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input request, AccountEntity expenseAccount, ExpenseEntity expenseToUpdate, CancellationToken cancellationToken);
}
