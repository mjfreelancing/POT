using AllOverIt.Patterns.Result;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create.Services;

public interface ICreateAccountService
{
    Task<EnrichedResult<AccountEntity>> CreateAccountAsync(Request request, CancellationToken cancellationToken);
}
