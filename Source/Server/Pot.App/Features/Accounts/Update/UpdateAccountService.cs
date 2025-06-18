using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Accounts.Update.EntityChecks;
using Pot.App.Features.Accounts.Update.Mappings;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Update;

internal sealed class UpdateAccountService : IUpdateAccountService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateAccountService(IPersistableAccountRepository accountRepository, IPreUpdateChecker preUpdateChecker, ILogger<UpdateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateAccountAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
        {
            var accountId = input.RowId;

            var accountToUpdate = await _accountRepository
                .GetAccountOrDefaultAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            if (accountToUpdate is null)
            {
                var accountNotFoundDetails = ProblemDetailsErrorFactory.CreateEntityNotFoundError("Account", nameof(AccountEntity.RowId), accountId);

                _logger.LogError(accountNotFoundDetails);

                return EnrichedResult.Fail<Output>(accountNotFoundDetails);
            }

            var problemDetails = await _preUpdateChecker
                .CanSaveAsync(input, accountToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (problemDetails is not null)
            {
                _logger.LogError(problemDetails);

                return EnrichedResult.Fail<Output>(problemDetails);
            }

            UpdateAccountEntity(accountToUpdate, input);

            // Not calling _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _accountRepository.SaveAsync(cancellationToken);

            var output = accountToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static void UpdateAccountEntity(AccountEntity accountToUpdate, Input request)
    {
        accountToUpdate.Bsb = request.Bsb;
        accountToUpdate.Number = request.Number;
        accountToUpdate.Description = request.Description;
        accountToUpdate.Balance = request.Balance;
        accountToUpdate.Reserved = request.Reserved;
        // account.TotalExpenseAccrued and account.DailyExpenseAccrual - will need to be re-calculated, when requested by the caller
    }
}
