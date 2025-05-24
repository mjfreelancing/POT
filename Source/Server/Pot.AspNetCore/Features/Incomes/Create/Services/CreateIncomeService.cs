using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Create.Services;

internal sealed class CreateIncomeService : ICreateIncomeService
{
    private readonly IPersistableIncomeRepository _incomeRepository;
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateIncomeService(IPersistableIncomeRepository incomeRepository, IPersistableAccountRepository accountRepository,
        IPreCreateChecker preCreateChecker, ILogger<CreateIncomeService> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<IncomeEntity>> CreateIncomeAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var incomeToCreate = new IncomeEntity
        {
            Description = request.Description,
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount
        };

        // incomeToCreate.Account will be assigned if it is found
        var canSaveResult = await _preCreateChecker.CanSaveAsync(request.AccountRowId, incomeToCreate, cancellationToken);

        if (canSaveResult is not null)
        {
            return canSaveResult.FailResult;
        }

        var account = incomeToCreate.Account;

        if (account is not null)
        {
            account.Incomes.Add(incomeToCreate);

            await _accountRepository
                .UpdateAndSaveAsync(account, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await _incomeRepository
                .AddAndSaveAsync(incomeToCreate, cancellationToken)
                .ConfigureAwait(false);
        }

        return EnrichedResult.Success(incomeToCreate);
    }
}
