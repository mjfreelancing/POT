using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create.Services;

public interface ICreateAccountService : IPotScopedDependency
{
    Task<EnrichedResult<AccountEntity>> CreateAccountAsync(Request request, CancellationToken cancellationToken);
}
