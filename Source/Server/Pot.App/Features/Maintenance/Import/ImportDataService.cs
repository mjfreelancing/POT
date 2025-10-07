using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Maintenance.Import.Accounts;
using Pot.App.Features.Maintenance.Import.Expenses;
using Pot.App.Features.Maintenance.Import.Incomes;
using Pot.App.Features.Maintenance.Import.Reader;
using Pot.App.Features.Maintenance.Metadata.Models;

namespace Pot.App.Features.Maintenance.Import;

internal sealed class ImportDataService : IImportDataService
{
    private readonly string[] _expectedEntryNames = ["metadata", "accounts", "incomes", "expenses"];
    private readonly IImportStreamReader _importStreamReader;
    private readonly IAccountsImporter _accountsImporter;
    private readonly IIncomesImporter _incomesImporter;
    private readonly IExpensesImporter _expensesImporter;
    private readonly ILogger _logger;

    public ImportDataService(IImportStreamReader importStreamReader, IAccountsImporter accountImporter,
        IIncomesImporter incomesImporter, IExpensesImporter expenseImporter, ILogger<ImportDataService> logger)
    {
        _importStreamReader = importStreamReader.WhenNotNull();
        _accountsImporter = accountImporter.WhenNotNull();
        _incomesImporter = incomesImporter.WhenNotNull();
        _expensesImporter = expenseImporter.WhenNotNull();
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

            var totalCount = 0;
            totalCount += await ImportAccountsAsync(cancellationToken);    // Must be first (incomes/expenses reference accounts)
            totalCount += await ImportExpensesAsync(cancellationToken);
            totalCount += await ImportIncomesAsync(cancellationToken);

            return EnrichedResult.Success(totalCount);
        }
    }

    private async Task<int> ImportAccountsAsync(CancellationToken cancellationToken)
    {
        using var accounts = _importStreamReader.GetAccounts();

        return await _accountsImporter.ImportAsync(accounts, cancellationToken);
    }

    private async Task<int> ImportExpensesAsync(CancellationToken cancellationToken)
    {
        using var expenses = _importStreamReader.GetExpenses();

        return await _expensesImporter.ImportAsync(expenses, cancellationToken);
    }

    private async Task<int> ImportIncomesAsync(CancellationToken cancellationToken)
    {
        using var incomes = _importStreamReader.GetIncomes();

        return await _incomesImporter.ImportAsync(incomes, cancellationToken);
    }
}
