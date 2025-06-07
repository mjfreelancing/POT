using Pot.App.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks;

public interface IPreCreateChecker : IPotScopedDependency
{
    Task<OutputState?> CanSaveAsync(IncomeEntity incomeToCreate, CancellationToken cancellationToken);
}
