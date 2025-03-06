using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Accounts.Update.Services.EntityChecks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Update.Services;

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

    public async Task<EnrichedResult<AccountEntity>> UpdateAccountAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
        {
            var accountId = request.RowId;

            var accountToUpdate = await _accountRepository
                .GetAccountOrDefaultAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            if (accountToUpdate is null)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(
                    ErrorCodes.NotFound,
                    nameof(AccountEntity.RowId),
                    accountId,
                    "The account does not exist");

                _logger.LogErrors(problemDetails);

                var accountError = new ServiceError(problemDetails);

                return EnrichedResult.Fail<AccountEntity>(accountError);
            }

            var canSaveResult = await _preUpdateChecker
                .CanSaveAsync(request, accountToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (canSaveResult is not null)
            {
                return canSaveResult.FailResult;
            }

            UpdateAccountEntity(accountToUpdate, request);

            // Don't call _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _accountRepository.SaveAsync(cancellationToken);

            return EnrichedResult.Success(accountToUpdate);
        }
    }

    private static void UpdateAccountEntity(AccountEntity accountToUpdate, Request request)
    {
        accountToUpdate.Bsb = request.Bsb;
        accountToUpdate.Number = request.Number;
        accountToUpdate.Description = request.Description;
        accountToUpdate.Balance = request.Balance;
        accountToUpdate.Reserved = request.Reserved;
        accountToUpdate.Allocated = request.Allocated;
        // account.DailyAccrual - will need to be re-calculated, when requested by the caller
    }
}
