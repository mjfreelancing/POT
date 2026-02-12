using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.App.Features.Incomes.Update.EntityChecks;
using Pot.App.Features.Incomes.Update.Mappings;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Incomes.Update;

internal sealed class UpdateIncomeService : IUpdateIncomeService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreUpdateChecker _preUpdateChecker;
    private readonly ILogger _logger;

    public UpdateIncomeService(IPersistableIncomeRepository incomeRepository, IPersistableAccountRepository accountRepository,
        IPreUpdateChecker preUpdateChecker, ILogger<UpdateIncomeService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull(); ;
        _preUpdateChecker = preUpdateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> UpdateIncomeAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_incomeRepository.WithTracking())
        {
            var incomeId = input.RowId;

            var incomeToUpdate = await _incomeRepository
                .GetIncomeOrDefaultAsync(incomeId, cancellationToken)
                .ConfigureAwait(false);

            if (incomeToUpdate is null)
            {
                var incomeNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(incomeId, "The income does not exist");

                _logger.LogApiError(incomeNotFoundError);

                return EnrichedResult.Fail<Output>(incomeNotFoundError);
            }

            var incomeAccount = await _accountRepository
                .GetAccountOrDefaultAsync(input.AccountRowId, cancellationToken)
                .ConfigureAwait(false);

            if (incomeAccount is null)
            {
                var incomeAccountNotFoundError = ApiDetailErrorFactory.CreateEntityNotFoundError(input.AccountRowId, "The account does not exist");

                _logger.LogApiError(incomeAccountNotFoundError);

                return EnrichedResult.Fail<Output>(incomeAccountNotFoundError);
            }

            var apiError = await _preUpdateChecker
                .CanSaveAsync(input, incomeAccount, incomeToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (apiError is not null)
            {
                _logger.LogApiError(apiError);

                return EnrichedResult.Fail<Output>(apiError);
            }

            UpdateIncomeEntity(incomeToUpdate, input, incomeAccount);

            // Not calling _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _incomeRepository.SaveAsync(cancellationToken);

            var output = incomeToUpdate.MapToOutput();

            return EnrichedResult.Success(output);
        }
    }

    private static void UpdateIncomeEntity(IncomeEntity incomeToUpdate, Input input, AccountEntity incomeAccount)
    {
        incomeToUpdate.ExcludeFromCalcs = input.ExcludeFromCalcs;
        incomeToUpdate.Description = input.Description;
        incomeToUpdate.NextDue = input.NextDue;
        incomeToUpdate.EndDate = input.EndDate;
        incomeToUpdate.Frequency = input.Frequency;
        incomeToUpdate.FrequencyCount = input.FrequencyCount;
        incomeToUpdate.Amount = input.Amount;
        incomeToUpdate.Note = input.Note;
        incomeToUpdate.Account = incomeAccount;
    }
}
