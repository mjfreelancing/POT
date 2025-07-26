using AllOverIt.Assertion;
using CsvHelper;
using Pot.App.Features.Maintenance.Import.Incomes.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;
using System.Globalization;

namespace Pot.App.Features.Maintenance.Import.Incomes;

internal sealed class IncomesImporter : IIncomesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IIncomeRepository _incomeRepository;

    public IncomesImporter(IPersistableAccountRepository accountRepository, IIncomeRepository incomeRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _incomeRepository = incomeRepository.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(zipStream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<IncomeCsvRow>().ToList();

        using (_accountRepository.WithTracking())
        {
            var accountRowIds = csvRows
                .Select(record => record.AccountRowId)
                .Distinct()
                .ToArray();

            var accounts = await _accountRepository
                .GetAccountsWithIncomesAsync(accountRowIds, cancellationToken)
                .ConfigureAwait(false);

            var accountLookup = accounts.ToDictionary(account => account.RowId, account => account);

            foreach (var csvRow in csvRows)
            {
                // Not validating the rows since the CSV file is expected to be valid (the data was previously exported).
                var account = accountLookup[csvRow.AccountRowId];

                await CreateOrUpdateIncomeAsync(account, csvRow, cancellationToken).ConfigureAwait(false);
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        return csvRows.Count;
    }

    private async Task CreateOrUpdateIncomeAsync(AccountEntity account, IncomeCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvExpenseId = csvRow.RowId;

        var incomeEntity = await _incomeRepository
            .GetIncomeOrDefaultAsync(csvExpenseId, cancellationToken)
            .ConfigureAwait(false);

        if (incomeEntity is null)
        {
            var income = CreateIncomeEntity(account, csvRow);
            account.Incomes.Add(income);
        }
        else
        {
            UpdateExistingIncome(incomeEntity, csvRow);
        }
    }

    private static IncomeEntity CreateIncomeEntity(AccountEntity account, IncomeCsvRow import)
    {
        var incomeEntity = new IncomeEntity
        {
            RowId = import.RowId,
            Description = import.Description,
            NextDue = import.NextDue,
            EndDate = import.EndDate,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Amount = import.Amount,
            Note = import.Note,
            Account = account
        };

        return incomeEntity;
    }

    private static void UpdateExistingIncome(IncomeEntity entity, IncomeCsvRow import)
    {
        entity.Description = import.Description;
        entity.NextDue = import.NextDue;
        entity.EndDate = import.EndDate;
        entity.Frequency = import.Frequency;
        entity.FrequencyCount = import.FrequencyCount;
        entity.Amount = import.Amount;
        entity.Note = import.Note;
    }
}
