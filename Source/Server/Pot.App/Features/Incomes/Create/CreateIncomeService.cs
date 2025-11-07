using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Incomes.Create.EntityChecks;
using Pot.App.Features.Incomes.Create.Mappings;
using Pot.App.Features.Incomes.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Incomes.Create;

internal sealed class CreateIncomeService : ICreateIncomeService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateIncomeService(IPersistableAccountRepository accountRepository, IPreCreateChecker preCreateChecker,
        ILogger<CreateIncomeService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> CreateIncomeAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var incomeAccount = await _accountRepository
            .GetAccountOrDefaultAsync(input.AccountRowId, cancellationToken)
            .ConfigureAwait(false);

        if (incomeAccount is null)
        {
            var incomeAccountProblem = ProblemDetailsErrorFactory.CreateEntityNotFoundError(input.AccountRowId, "The account does not exist.");

            return EnrichedResult.Fail<Output>(incomeAccountProblem);
        }

        var incomeToCreate = new IncomeEntity
        {
            ExcludeFromCalcs = input.ExcludeFromCalcs,  // Will be false for new incomes, may be true for imported data
            Description = input.Description,
            NextDue = input.NextDue,
            EndDate = input.EndDate,
            Frequency = input.Frequency,
            FrequencyCount = input.FrequencyCount,
            Amount = input.Amount,
            Note = input.Note,
            Account = incomeAccount
        };

        // Provided when importing
        if (input.RowId.HasValue)
        {
            incomeToCreate.RowId = input.RowId.Value;
        }

        var problemDetails = await _preCreateChecker.CanSaveAsync(incomeToCreate, cancellationToken);

        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        incomeAccount.Incomes.Add(incomeToCreate);

        await _accountRepository
            .UpdateAndSaveAsync(incomeAccount, cancellationToken)
            .ConfigureAwait(false);

        var createdIncome = incomeToCreate.MapToOutput();

        return EnrichedResult.Success(createdIncome);
    }
}
