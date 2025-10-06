using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Maintenance.Import.Incomes;

internal sealed class IncomesImporter : IIncomesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger<IncomesImporter> _logger;

    public IncomesImporter(IPersistableAccountRepository accountRepository, IIncomeRepository incomeRepository, ILogger<IncomesImporter> logger)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<int> ImportAsync(IEnumerable<IIncomeCsvRow> csvRows, CancellationToken cancellationToken)
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

                await CreateOrUpdateIncomeAsync(account, csvRow, cancellationToken).ConfigureAwait(false);

                count++;
            }

            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        return count;
    }

    private async Task CreateOrUpdateIncomeAsync(AccountEntity account, IIncomeCsvRow csvRow, CancellationToken cancellationToken)
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

    private static IncomeEntity CreateIncomeEntity(AccountEntity account, IIncomeCsvRow import)
    {
        var incomeEntity = new IncomeEntity
        {
            RowId = import.RowId,
            ExcludeFromCalcs = import.ExcludeFromCalcs,
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

    private static void UpdateExistingIncome(IncomeEntity entity, IIncomeCsvRow import)
    {
        entity.ExcludeFromCalcs = import.ExcludeFromCalcs;
        entity.Description = import.Description;
        entity.NextDue = import.NextDue;
        entity.EndDate = import.EndDate;
        entity.Frequency = import.Frequency;
        entity.FrequencyCount = import.FrequencyCount;
        entity.Amount = import.Amount;
        entity.Note = import.Note;
    }
}
