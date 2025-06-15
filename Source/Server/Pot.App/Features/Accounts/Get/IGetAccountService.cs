using Pot.App.Features.Accounts.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Get;

public interface IGetAccountService : IPotScopedDependency
{
    Task<Output?> GetAccountAsync(Guid id, CancellationToken cancellationToken);
}
