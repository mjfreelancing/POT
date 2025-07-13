using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Create.EntityChecks;
using Pot.App.Features.Expenses.Create.Mappings;
using Pot.App.Features.Expenses.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Expenses.Create;

internal sealed class CreateExpenseService : ICreateExpenseService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ILogger _logger;

    public CreateExpenseService(IPersistableAccountRepository accountRepository, IPreCreateChecker preCreateChecker,
        ILogger<CreateExpenseService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> CreateExpenseAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expenseAccount = await _accountRepository
            .GetAccountOrDefaultAsync(input.AccountRowId, cancellationToken)
            .ConfigureAwait(false);

        if (expenseAccount is null)
        {
            var expenseAccountProblem = ProblemDetailsErrorFactory.CreateEntityNotFoundError(input.AccountRowId, "The account does not exist.");

            return EnrichedResult.Fail<Output>(expenseAccountProblem);
        }

        var expenseToCreate = new ExpenseEntity
        {
            Description = input.Description,
            NextDue = input.NextDue,
            AccrualStart = input.AccrualStart,
            EndDate = input.EndDate,
            Frequency = input.Frequency,
            FrequencyCount = input.FrequencyCount,
            Amount = input.Amount,
            Note = input.Note,
            Account = expenseAccount,
        };

        var problemDetails = await _preCreateChecker.CanSaveAsync(expenseToCreate, cancellationToken);

        if (problemDetails is not null)
        {
            return EnrichedResult.Fail<Output>(problemDetails);
        }

        expenseAccount.Expenses.Add(expenseToCreate);

        await _accountRepository
            .UpdateAndSaveAsync(expenseAccount, cancellationToken)
            .ConfigureAwait(false);

        var createdExpense = expenseToCreate.MapToOutput();

        return EnrichedResult.Success(createdExpense);
    }
}
