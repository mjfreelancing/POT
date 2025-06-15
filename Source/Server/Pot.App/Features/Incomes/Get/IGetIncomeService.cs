using Pot.App.Features.Incomes.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Get;

public interface IGetIncomeService : IPotScopedDependency
{
    Task<Output?> GetIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
