using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Errors;
using Pot.App.Features.Expenses.Create.EntityChecks;
using Pot.App.Features.Expenses.Create.Mappings;
using Pot.App.Features.Expenses.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Shared.Extensions;

namespace Pot.App.Features.Expenses.Create;

internal sealed class CreateExpenseService : ICreateExpenseService
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IPreCreateChecker _preCreateChecker;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public CreateExpenseService(IPersistableAccountRepository accountRepository, IPreCreateChecker preCreateChecker,
        ITimeProvider timeProvider, ILogger<CreateExpenseService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _preCreateChecker = preCreateChecker.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
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
            var expenseAccountError = ApiDetailErrorFactory.CreateEntityNotFoundError(input.AccountRowId, "The account does not exist");

            return EnrichedResult.Fail<Output>(expenseAccountError);
        }

        var expenseToCreate = new ExpenseEntity
        {
            ExcludeFromCalcs = input.ExcludeFromCalcs,  // Will be false for new expenses, may be true for imported data
            Description = input.Description,
            NextDue = input.NextDue,
            AccrualStart = input.AccrualPolicy.GetCanonicalAccrualStart(input.AccrualStart, _timeProvider.GetLocalDateNow()),
            EndDate = input.EndDate,
            AccrualPolicy = input.AccrualPolicy,
            Frequency = input.Frequency,
            FrequencyCount = input.FrequencyCount,
            Amount = input.Amount,
            Note = input.Note,
            Account = expenseAccount,
            AccruedIsDirty = true                       // The default is true, but being explicit
        };

        // Provided when importing
        if (input.RowId.HasValue)
        {
            expenseToCreate.RowId = input.RowId.Value;
        }

        var apiError = await _preCreateChecker.CanSaveAsync(expenseToCreate, cancellationToken);

        if (apiError is not null)
        {
            return EnrichedResult.Fail<Output>(apiError);
        }

        expenseAccount.Expenses.Add(expenseToCreate);

        await _accountRepository
            .UpdateAndSaveAsync(expenseAccount, cancellationToken)
            .ConfigureAwait(false);

        var createdExpense = expenseToCreate.MapToOutput();

        return EnrichedResult.Success(createdExpense);
    }
}
