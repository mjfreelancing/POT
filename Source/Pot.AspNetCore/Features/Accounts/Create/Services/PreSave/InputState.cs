using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.PreSave;

internal sealed class InputState
{
    public required AccountEntity AccountToCreate { get; init; }
    public required IAccountRepository AccountRepository { get; init; }
    public required ILogger Logger { get; init; }
}
