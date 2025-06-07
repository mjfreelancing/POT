using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Accounts.GetAll.Models;

namespace Pot.App.Features.Accounts.GetAll;

public interface IGetAllAccountsService : IPotScopedDependency
{
    Task<List<Output>> GetAllAccountsAsync(CancellationToken cancellationToken);
}
