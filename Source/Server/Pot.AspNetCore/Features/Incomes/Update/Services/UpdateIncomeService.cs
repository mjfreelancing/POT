using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Update.Services;

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

    public async Task<EnrichedResult<IncomeEntity>> UpdateIncomeAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_incomeRepository.WithTracking())
        {
            var incomeId = request.RowId;

            var incomeToUpdate = await _incomeRepository
                .GetIncomeOrDefaultAsync(incomeId, cancellationToken)
                .ConfigureAwait(false);

            if (incomeToUpdate is null)
            {
                return IncomeDoesNotExist(incomeId);
            }

            var incomeAccount = await _accountRepository
                .GetAccountOrDefaultAsync(request.AccountRowId, cancellationToken)
                .ConfigureAwait(false);

            if (incomeAccount is null)
            {
                return IncomeAccountDoesNotExist(request.AccountRowId);
            }

            var canSaveResult = await _preUpdateChecker
                .CanSaveAsync(request, incomeAccount, incomeToUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (canSaveResult is not null)
            {
                return canSaveResult.FailResult;
            }

            UpdateIncomeEntity(incomeToUpdate, request, incomeAccount);

            // Not calling _accountRepository.Update(account) as this will mark the
            // entity as modified even if nothing was changed.
            _ = await _incomeRepository.SaveAsync(cancellationToken);

            return EnrichedResult.Success(incomeToUpdate);
        }
    }

    private EnrichedResult<IncomeEntity> IncomeDoesNotExist(Guid incomeId)
    {
        var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(
            ErrorCodes.NotFound,
            nameof(IncomeEntity.RowId),
            incomeId,
            "The income does not exist");

        _logger.LogErrors(problemDetails);

        var incomeError = new ServiceError(problemDetails);

        return EnrichedResult.Fail<IncomeEntity>(incomeError);
    }

    private EnrichedResult<IncomeEntity> IncomeAccountDoesNotExist(Guid accountRowId)
    {
        var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(
                ErrorCodes.NotFound,
                nameof(Request.AccountRowId),
                accountRowId,
                "The account does not exist");

        _logger.LogErrors(problemDetails);

        var incomeError = new ServiceError(problemDetails);

        return EnrichedResult.Fail<IncomeEntity>(incomeError);
    }

    private static void UpdateIncomeEntity(IncomeEntity incomeToUpdate, Request request, AccountEntity incomeAccount)
    {
        incomeToUpdate.Description = request.Description;
        incomeToUpdate.NextDue = request.NextDue;
        incomeToUpdate.EndDate = request.EndDate;
        incomeToUpdate.Frequency = request.Frequency;
        incomeToUpdate.FrequencyCount = request.FrequencyCount;
        incomeToUpdate.Amount = request.Amount;
        incomeToUpdate.Account = incomeAccount;
    }
}
