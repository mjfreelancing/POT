using AllOverIt.Assertion;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.App.Concerns.Csv;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;
using Pot.Shared;
using Pot.Shared.DependencyInjection;
using System.Globalization;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Import;

public interface IImportDataService : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}

internal sealed class ImportDataService : IImportDataService
{
    private readonly IAccountsImporter _accountsImporter;
    private readonly IIncomesImporter _incomesImporter;
    private readonly IExpensesImporter _expensesImporter;

    public ImportDataService(IAccountsImporter accountImporter, IIncomesImporter incomesImporter, IExpensesImporter expenseImporter)
    {
        _accountsImporter = accountImporter.WhenNotNull();
        _incomesImporter = incomesImporter.WhenNotNull();
        _expensesImporter = expenseImporter.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        _ = zipStream.WhenNotNull();

        var totalCount = 0;

        using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);

        var entries = archive.Entries.ToDictionary(kvp => kvp.Name);

        async Task<int> HandleEntry(ZipArchiveEntry entry, Func<Stream, CancellationToken, Task<int>> handler)
        {
            using var stream = entry.Open();
            return await handler.Invoke(stream, cancellationToken);
        }

        // TODO: Add versioning to the zip file.
        // TODO: Add asymmetric encryption to the zip file (export and import).
        // TODO: Add validation to ensure all required entries are present.
        // TODO: Add validation per importer.

        // Process in the required order
        totalCount += await HandleEntry(entries["accounts"], _accountsImporter.ImportAsync);
        totalCount += await HandleEntry(entries["incomes"], _incomesImporter.ImportAsync);
        totalCount += await HandleEntry(entries["expenses"], _expensesImporter.ImportAsync);

        return totalCount;
    }
}

internal sealed class AccountCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Bsb { get; init; } = string.Empty;

    [Index(2)]
    public string Number { get; init; } = string.Empty;

    [Index(3)]
    public string Description { get; init; } = string.Empty;

    [Index(4)]
    public double Balance { get; init; }

    [Index(5)]
    public double Reserved { get; init; }

    [Index(6)]
    public double TotalExpenseAccrued { get; init; }

    [Index(7)]
    public double DailyExpenseAccrual { get; init; }
}

public interface IAccountsImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}

internal sealed class AccountsImporter : IAccountsImporter
{
    private readonly IPersistableAccountRepository _accountRepository;

    public AccountsImporter(IPersistableAccountRepository accountRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(zipStream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<AccountCsvRow>().ToList();

        using (_accountRepository.WithTracking())
        {
            foreach (var csvRow in csvRows)
            {
                await CreateOrUpdateAccountAsync(csvRow, cancellationToken).ConfigureAwait(false);
            }

            // Could throw UniqueConstraintException (or a related database exception),
            // resulting in a custom 422 ProblemDetails response.
            await _accountRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return csvRows.Count;
    }

    private async Task CreateOrUpdateAccountAsync(AccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        var csvAccountId = csvRow.RowId;

        var accountEntity = await _accountRepository
            .GetAccountOrDefaultAsync(csvAccountId, cancellationToken)
            .ConfigureAwait(false);

        if (accountEntity is null)
        {
            CreateAccountEntity(csvRow);
        }
        else
        {
            UpdateExistingAccount(accountEntity, csvRow);
        }
    }

    private AccountEntity CreateAccountEntity(AccountCsvRow import)
    {
        var accountEntity = new AccountEntity
        {
            RowId = import.RowId,
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            TotalExpenseAccrued = import.TotalExpenseAccrued,
            DailyExpenseAccrual = import.DailyExpenseAccrual
        };

        _accountRepository.Add(accountEntity);

        return accountEntity;
    }

    private static void UpdateExistingAccount(AccountEntity entity, AccountCsvRow import)
    {
        // Don't need to explicitly call _accountRepository.Update(entity).
        // The entity will be marked as modified if anything has changed.
        entity.Bsb = import.Bsb;
        entity.Number = import.Number;
        entity.Description = import.Description;
        entity.Balance = import.Balance;
        entity.Reserved = import.Reserved;
        entity.TotalExpenseAccrued = import.TotalExpenseAccrued;
        entity.DailyExpenseAccrual = import.DailyExpenseAccrual;
    }
}








internal sealed class ExpenseCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Description { get; init; } = string.Empty;

    [Index(2)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly AccrualStart { get; init; }

    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    [Index(4)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    [Index(5)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    [Index(6)]
    public int FrequencyCount { get; init; }

    [Index(7)]
    public double Amount { get; init; }

    [Index(8)]
    public double Accrued { get; init; }

    [Index(9)]
    public string Note { get; init; } = string.Empty;

    [Index(10)]
    public Guid AccountRowId { get; init; }
}

public interface IExpensesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}

internal sealed class ExpensesImporter : IExpensesImporter
{
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly IExpenseRepository _expenseRepository;

    public ExpensesImporter(IPersistableAccountRepository accountRepository, IExpenseRepository expenseRepository)
    {
        _accountRepository = accountRepository.WhenNotNull();
        _expenseRepository = expenseRepository.WhenNotNull();
    }

    public async Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(zipStream);

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
                // TODO: Check for unknown account
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









internal sealed class IncomeCsvRow
{
    [Index(0)]
    public Guid RowId { get; init; }

    [Index(1)]
    public string Description { get; init; } = string.Empty;

    [Index(2)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(NullableDateOnlyConverter))]
    public DateOnly? EndDate { get; init; }

    [Index(4)]
    [TypeConverter(typeof(FrequencyConverter))]
    public required Frequency Frequency { get; init; }

    [Index(5)]
    public int FrequencyCount { get; init; }

    [Index(6)]
    public double Amount { get; init; }

    [Index(7)]
    public string Note { get; init; } = string.Empty;

    [Index(8)]
    public Guid AccountRowId { get; init; }
}

public interface IIncomesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}

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
                // TODO: Check for unknown account
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
