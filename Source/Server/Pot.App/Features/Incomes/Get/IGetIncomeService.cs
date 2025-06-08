using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Incomes.Get.Models;

namespace Pot.App.Features.Incomes.Get;

public interface IGetIncomeService : IPotScopedDependency
{
    Task<Output?> GetIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
