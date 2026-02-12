using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Create.EntityChecks;

public interface IPreCreateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(IncomeEntity incomeToCreate, CancellationToken cancellationToken);
}
