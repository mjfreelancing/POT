using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.Get.Models;

namespace Pot.App.Features.Accounts.Get;

public interface IGetAccountService : IPotScopedDependency
{
    Task<Output?> GetAccountAsync(Guid id, CancellationToken cancellationToken);
}
