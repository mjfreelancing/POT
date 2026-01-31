using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accounts.Delete.EntityChecks;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Delete;

internal sealed class DeleteAccountService : IDeleteAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreDeleteChecker _preDeleteChecker;
    private readonly ILogger _logger;

    public DeleteAccountService(IPersistableAccountRepository accountRepository, IPreDeleteChecker preCreateChecker, ILogger<DeleteAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preDeleteChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<bool>> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var problemDetails = await _preDeleteChecker.CanDeleteAsync(accountId, cancellationToken);

        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<bool>(problemDetails);
        }

        var account = await _accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

        if (account is null)
        {
            var accountNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError(accountId, "The account does not exist");

            _logger.LogError(accountNotFoundDetails);

            return EnrichedResult.Fail<bool>(accountNotFoundDetails);
        }

        _accountRepository.Delete(account);

        await _accountRepository.SaveAsync(cancellationToken);

        return EnrichedResult.Success(true);
    }
}
