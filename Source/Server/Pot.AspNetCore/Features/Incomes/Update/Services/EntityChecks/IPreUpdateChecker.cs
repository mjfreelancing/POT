using Pot.App.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<OutputState?> CanSaveAsync(Request request, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken);
}
