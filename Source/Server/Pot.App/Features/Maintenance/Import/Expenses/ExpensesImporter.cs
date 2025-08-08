using AllOverIt.Assertion;
using CsvHelper;
using Pot.App.Features.Maintenance.Import.Expenses.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using System.Globalization;

namespace Pot.App.Features.Maintenance.Import.Expenses;

internal sealed class ExpensesImporter : IExpensesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IExpenseRepository _expenseRepository;

    public ExpensesImporter(IPersistableAccountRepository accountRepository, IExpenseRepository expenseRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream dataStream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(dataStream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<ExpenseCsvRow>().ToList();

        using (_accountRepository.WithTracking())
        {
            var accountRowIds = csvRows
                .Select(record => record.AccountRowId)
                .Distinct()
                .ToArray();

            var accounts = await _accountRepository
                .GetAccountsWithExpensesAsync(accountRowIds, cancellationToken)
                .ConfigureAwait(false);

            var accountLookup = accounts.ToDictionary(account => account.RowId, account => account);

            foreach (var csvRow in csvRows)
            {
                // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
                var account = accountLookup[csvRow.AccountRowId];

                await CreateOrUpdateExpenseAsync(account, csvRow, cancellationToken).ConfigureAwait(false);
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        return csvRows.Count;
    }

    private async Task CreateOrUpdateExpenseAsync(AccountEntity account, ExpenseCsvRow csvRow, CancellationToken cancellationToken)
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

    private static ExpenseEntity CreateExpenseEntity(AccountEntity account, ExpenseCsvRow import)
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
            Note = import.Note,
            Account = account
        };

        return expenseEntity;
    }

    private static void UpdateExistingExpense(ExpenseEntity entity, ExpenseCsvRow import)
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
        entity.Note = import.Note;
    }
}
