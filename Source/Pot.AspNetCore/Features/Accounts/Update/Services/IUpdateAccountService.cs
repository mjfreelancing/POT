using AllOverIt.Patterns.Result;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services;

public interface IUpdateAccountService
{
    Task<EnrichedResult<AccountEntity>> UpdateAccountAsync(Request request, CancellationToken cancellationToken);
}
