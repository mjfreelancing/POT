using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Delete.EntityChecks;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Delete;

internal sealed class DeleteAccountService : IDeleteAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public DeleteAccountService(IPersistableAccountRepository accountRepository, IPreCreateChecker preCreateChecker, ILogger<DeleteAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var problemDetails = await _preCreateChecker.CanSaveAsync(accountId, cancellationToken);

        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<bool>(problemDetails);
        }

        var account = await _accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

        if (account is null)
        {
            return EnrichedResult.Success(false);
        }

        _accountRepository.Delete(account);

        await _accountRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(true);
    }
}
