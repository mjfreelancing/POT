using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Errors;
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

        var incomeAccount = await _accountRepository
            .GetAccountOrDefaultAsync(request.AccountRowId, cancellationToken)
            .ConfigureAwait(false);

        if (incomeAccount is null)
        {
            var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(
                ErrorCodes.NotFound,
                nameof(Request.AccountRowId),
                request.AccountRowId,
                "The account does not exist");

            _logger.LogErrors(problemDetails);

            var incomeError = new ServiceError(problemDetails);

            return EnrichedResult.Fail<IncomeEntity>(incomeError);
        }

        var incomeToCreate = new IncomeEntity
        {
            Description = request.Description,
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency,
            FrequencyCount = request.FrequencyCount,
            Amount = request.Amount,
            Account = incomeAccount
        };

        var canSaveResult = await _preCreateChecker.CanSaveAsync(incomeToCreate, cancellationToken);

        if (canSaveResult is not null)
        {
            return canSaveResult.FailResult;
        }

        incomeAccount.Incomes.Add(incomeToCreate);

        await _accountRepository
            .UpdateAndSaveAsync(incomeAccount, cancellationToken)
            .ConfigureAwait(false);

        return EnrichedResult.Success(incomeToCreate);
    }
}
