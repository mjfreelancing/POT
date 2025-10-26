using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Maintenance.Import.Accounts;
using Pot.App.Features.Maintenance.Import.Expenses;
using Pot.App.Features.Maintenance.Import.Incomes;
using Pot.App.Features.Maintenance.Import.Reader;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Data;

namespace Pot.App.Features.Maintenance.Import;

internal sealed class ImportDataService : IImportDataService
{
    private readonly string[] _expectedEntryNames = ["metadata", "accounts", "incomes", "expenses"];
    private readonly IImportStreamReader _importStreamReader;
    private readonly IAccountsImporter _accountsImporter;
    private readonly IIncomesImporter _incomesImporter;
    private readonly IExpensesImporter _expensesImporter;
    private readonly IPotTransactionFactory _transactionFactory;
    private readonly ILogger _logger;

    public ImportDataService(IImportStreamReader importStreamReader, IAccountsImporter accountImporter,
        IIncomesImporter incomesImporter, IExpensesImporter expenseImporter, IPotTransactionFactory transactionFactory,
        ILogger<ImportDataService> logger)
    {
        _importStreamReader = importStreamReader.WhenNotNull();
        _accountsImporter = accountImporter.WhenNotNull();
        _incomesImporter = incomesImporter.WhenNotNull();
        _expensesImporter = expenseImporter.WhenNotNull();
        _transactionFactory = transactionFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        _ = zipStream.WhenNotNull();

        _logger.LogCall(this);

        using (_importStreamReader.Open(zipStream))
        {
            if (!_expectedEntryNames.All(_importStreamReader.EntryNames.Contains))
            {
                var problemDetailsError = ProblemDetailsErrorFactory.CreateUnprocessableEntityError("The import file is not supported.");

                return EnrichedResult.Fail<int>(problemDetailsError);
            }

            MetadataBase metadataBase = _importStreamReader.GetMetadata();

            // There's only 1 version at the moment
            if (metadataBase.Version == 1)
            {
                var metadataV1 = (MetadataV1)metadataBase;
                _logger.LogInformation("Importing data (as v{MetadataVersion}) from {MetadataCreatedAt}", metadataV1.Version, metadataV1.CreatedAt);
            }

            using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);

            // Must be first (incomes/expenses reference accounts)
            var accountsResult = await ImportAccountsAsync(cancellationToken);

            if (accountsResult.IsFail)
            {
                return EnrichedResult.Fail<int>(accountsResult.Error);
            }

            var expensesResult = await ImportExpensesAsync(cancellationToken);

            if (expensesResult.IsFail)
            {
                return EnrichedResult.Fail<int>(expensesResult.Error);
            }

            var incomesResult = await ImportIncomesAsync(cancellationToken);

            if (incomesResult.IsFail)
            {
                return EnrichedResult.Fail<int>(incomesResult.Error);
            }

            await transaction.CommitAsync(cancellationToken);

            var totalCount = accountsResult.Value + expensesResult.Value + incomesResult.Value;

            return EnrichedResult.Success(totalCount);
        }
    }

    private async Task<EnrichedResult<int>> ImportAccountsAsync(CancellationToken cancellationToken)
    {
        using var accounts = _importStreamReader.GetAccounts();

        return await _accountsImporter.ImportAsync(accounts, cancellationToken);
    }

    private async Task<EnrichedResult<int>> ImportExpensesAsync(CancellationToken cancellationToken)
    {
        using var expenses = _importStreamReader.GetExpenses();

        return await _expensesImporter.ImportAsync(expenses, cancellationToken);
    }

    private async Task<EnrichedResult<int>> ImportIncomesAsync(CancellationToken cancellationToken)
    {
        using var incomes = _importStreamReader.GetIncomes();

        return await _incomesImporter.ImportAsync(incomes, cancellationToken);
    }
}
