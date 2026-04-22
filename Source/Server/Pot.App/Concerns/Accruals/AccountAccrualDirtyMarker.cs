using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.Data.Entities;
using Pot.Data.Repositories.AccountAccrual;

namespace Pot.App.Concerns.Accruals;

internal sealed class AccountAccrualDirtyMarker : IAccountAccrualDirtyMarker
{
    private readonly IPersistableAccountAccrualRepository _accountAccrualRepository;
    private readonly ILogger<AccountAccrualDirtyMarker> _logger;

    public AccountAccrualDirtyMarker(IPersistableAccountAccrualRepository accountAccrualRepository, ILogger<AccountAccrualDirtyMarker> logger)
    {
        _accountAccrualRepository = accountAccrualRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task MarkDirtyForCreateAsync(AccountEntity account, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        _ = account.WhenNotNull();

        var accountAccrual = await _accountAccrualRepository
            .Set<AccountAccrualEntity>()
            .SingleOrDefaultAsync(item => item.AccountId == account.Id, cancellationToken)
            .ConfigureAwait(false);

        if (accountAccrual is null)
        {
            accountAccrual = new AccountAccrualEntity
            {
                AccountId = account.Id,
                Account = account,
                AccruedIsDirty = true
            };

            _accountAccrualRepository.Add(accountAccrual);

            return;
        }

        if (!accountAccrual.AccruedIsDirty)
        {
            accountAccrual.AccruedIsDirty = true;

            _accountAccrualRepository.Update(accountAccrual);
        }
    }
}
