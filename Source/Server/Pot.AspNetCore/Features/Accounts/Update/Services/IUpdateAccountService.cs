using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services;

public interface IUpdateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<AccountEntity>> UpdateAccountAsync(Request request, CancellationToken cancellationToken);
}
