using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.EntityChecks;

internal sealed class InputState
{
    public required Request Request { get; init; }
    public required AccountEntity AccountToUpdate { get; init; }
    public required IAccountRepository AccountRepository { get; init; }
    public required ILogger Logger { get; init; }
}
