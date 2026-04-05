using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using CsvHelper;
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

        try
        {
            using (_importStreamReader.Open(zipStream))
            {
                if (!_expectedEntryNames.All(_importStreamReader.EntryNames.Contains))
                {
                    var unsupportedFileError = ApiDetailErrorFactory.CreateUnprocessableEntityError("The import file is not supported");

                    return EnrichedResult.Fail<int>(unsupportedFileError);
                }

                var metadataVersion = _importStreamReader.ReadMetadataVersion();

                // v2 added 'AccrualPolicy' to the export due to a schema change. We cannot import v1 without guessing or making assumptions
                // around the rules to apply. For newly imported expenses, we could have applied AccrualPolicy = 'None' for expenses with a
                // Frequency of 'OneTime' and AccrualPolicy = 'Automatic' for all others, but the user may not review the defaults applied.
                // It's safer to reject the import. Worst case scenario is the user needs to manually inspect the export file and enter the
                // expenses manually. If this becomes an issue in the future, we could create an import preview page that allows the user
                // to review the data and make adjustments before importing.
                if (metadataVersion != MetadataBase.CurrentVersion)
                {
                    var unsupportedMetadataVersionError =
                        ApiDetailErrorFactory.CreateUnprocessableEntityError(nameof(MetadataBase.Version), metadataVersion,
                            $"The import file has a non-supported version. Expecting version {MetadataBase.CurrentVersion}.");

                    return EnrichedResult.Fail<int>(unsupportedMetadataVersionError);
                }

                var metadataBase = _importStreamReader.GetMetadata();

                var metadataV2 = (MetadataV2)metadataBase;
                _logger.LogInformation("Importing data (as v{MetadataVersion}) from {MetadataCreatedAt}", metadataV2.Version, metadataV2.CreatedAt);

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
        catch (Exception exception) when (IsInvalidImportPayloadException(exception))
        {
            _logger.LogWarning(exception, "Import failed due to unsupported or invalid payload format");

            var invalidPayloadError = ApiDetailErrorFactory.CreateUnprocessableEntityError("The import file does not have a valid format.");

            return EnrichedResult.Fail<int>(invalidPayloadError);
        }
    }

    private static bool IsInvalidImportPayloadException(Exception exception)
    {
        return exception is CsvHelperException or InvalidDataException or FormatException or
                            OverflowException or EndOfStreamException or NotSupportedException;
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
