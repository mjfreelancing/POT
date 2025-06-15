using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.GetAll;

public interface IGetAllAccountsService : IPotScopedDependency
{
    Task<List<Output>> GetAllAccountsAsync(CancellationToken cancellationToken);
}
