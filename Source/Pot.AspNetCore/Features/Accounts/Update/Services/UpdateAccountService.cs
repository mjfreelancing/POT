using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Features.Accounts.Update.Services.PreSave;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Update.Services;

internal sealed class UpdateAccountService : IUpdateAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPreUpdateChecker _preUpdateCommitChecker;
    private readonly ILogger _logger;

    public UpdateAccountService(IAccountRepository accountRepository, IPreUpdateChecker preUpdateCommitChecker, ILogger<UpdateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preUpdateCommitChecker = preUpdateCommitChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<AccountEntity>> UpdateAccountAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
        {
            var accountToUpdate = await _accountRepository.GetAccountAsync(request.RowId.As<Guid>(), cancellationToken);

            var canSaveResult = await _preUpdateCommitChecker.CanSaveAsync(request, accountToUpdate, cancellationToken);

            if (canSaveResult is not null)
            {
                return canSaveResult.FailResult;
            }

            accountToUpdate.Bsb = request.Bsb;
            accountToUpdate.Number = request.Number;
            accountToUpdate.Description = request.Description;
            accountToUpdate.Balance = request.Balance;
            accountToUpdate.Reserved = request.Reserved;
            accountToUpdate.Allocated = request.Allocated;
            // account.DailyAccrual - will need to be re-calculated, when requested by the caller

            // Don't call _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _accountRepository.SaveAsync(cancellationToken);

            return EnrichedResult.Success(accountToUpdate);
        }
    }
}
