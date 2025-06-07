using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accounts.Create;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Delete;

internal sealed class DeleteAccountService : IDeleteAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public DeleteAccountService(IPersistableAccountRepository accountRepository, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // TODO: Don't allow deletion of accounts with associated expenses / incomes.
        //       Consider soft deletes. ? Reporting requirements.
        //       Decide when the need arises.

        var account = await _accountRepository.GetAccountOrDefaultAsync(id, cancellationToken);

        if (account is null)
        {
            return false;
        }

        _accountRepository.Delete(account);

        await _accountRepository.SaveAsync(cancellationToken);

        return true;
    }
}
