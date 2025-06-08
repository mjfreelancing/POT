using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Create.EntityChecks;

public interface IPreCreateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(IncomeEntity incomeToCreate, CancellationToken cancellationToken);
}
