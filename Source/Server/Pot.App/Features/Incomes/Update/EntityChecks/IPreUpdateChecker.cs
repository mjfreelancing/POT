using Pot.App.Errors;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input request, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken);
}
