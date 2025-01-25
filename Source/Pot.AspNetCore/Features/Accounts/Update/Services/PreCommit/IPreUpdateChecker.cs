using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreCommit;

public interface IPreUpdateChecker
{
    Task<OutputState?> CanSaveAsync(Request request, AccountEntity accountToUpdate, CancellationToken cancellationToken);
}
