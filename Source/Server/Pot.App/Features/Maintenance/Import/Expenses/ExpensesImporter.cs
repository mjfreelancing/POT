using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Features.Maintenance.Import.Expenses;

internal sealed class ExpensesImporter : IExpensesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger<ExpensesImporter> _logger;

    public ExpensesImporter(IPersistableAccountRepository accountRepository, IExpenseRepository expenseRepository, ILogger<ExpensesImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<int> ImportAsync(IEnumerable<IExpenseCsvRow> csvRows, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var count = 0;

        using (_accountRepository.WithTracking())
        {
            var accounts = new Dictionary<Guid, AccountEntity>();

            foreach (var csvRow in csvRows)
            {
                if (!accounts.TryGetValue(csvRow.AccountRowId, out var account))
                {
                    // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
                    account = await _accountRepository
                        .GetAccountAsync(csvRow.AccountRowId, cancellationToken)
                        .ConfigureAwait(false);

                    accounts.Add(csvRow.AccountRowId, account);
                }

                await CreateOrUpdateExpenseAsync(account, csvRow, cancellationToken).ConfigureAwait(false);

                count++;
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        return count;
    }

    private async Task CreateOrUpdateExpenseAsync(AccountEntity account, IExpenseCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvExpenseId = csvRow.RowId;

        var expenseEntity = await _expenseRepository
            .GetExpenseOrDefaultAsync(csvExpenseId, cancellationToken)
            .ConfigureAwait(false);

        if (expenseEntity is null)
        {
            var expense = CreateExpenseEntity(account, csvRow);
            account.Expenses.Add(expense);
        }
        else
        {
            UpdateExistingExpense(expenseEntity, csvRow);
        }
    }

    private static ExpenseEntity CreateExpenseEntity(AccountEntity account, IExpenseCsvRow import)
    {
        var expenseEntity = new ExpenseEntity
        {
            RowId = import.RowId,
            ExcludeFromCalcs = import.ExcludeFromCalcs,
            Description = import.Description,
            AccrualStart = import.AccrualStart,
            NextDue = import.NextDue,
            EndDate = import.EndDate,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Amount = import.Amount,
            Accrued = import.Accrued,
            AccruedIsDirty = import.AccruedIsDirty,
            LastAccruedUpdate = import.LastAccruedUpdate,
            Note = import.Note,
            Account = account
        };

        return expenseEntity;
    }

    private static void UpdateExistingExpense(ExpenseEntity entity, IExpenseCsvRow import)
    {
        entity.ExcludeFromCalcs = import.ExcludeFromCalcs;
        entity.Description = import.Description;
        entity.AccrualStart = import.AccrualStart;
        entity.NextDue = import.NextDue;
        entity.EndDate = import.EndDate;
        entity.Frequency = import.Frequency;
        entity.FrequencyCount = import.FrequencyCount;
        entity.Amount = import.Amount;
        entity.Accrued = import.Accrued;
        entity.AccruedIsDirty = import.AccruedIsDirty;
        entity.LastAccruedUpdate = import.LastAccruedUpdate;
        entity.Note = import.Note;
    }
}
