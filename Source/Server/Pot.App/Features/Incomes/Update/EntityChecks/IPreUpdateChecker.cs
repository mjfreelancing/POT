using Pot.App.Concerns.DependencyInjection;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input request, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken);
}
