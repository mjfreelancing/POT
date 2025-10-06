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
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Import;

internal sealed class ImportDataService : IImportDataService
{
    private readonly string[] _expectedEntryNames = ["metadata", "accounts", "incomes", "expenses"];
    private readonly IImportStreamReaderFactory _importStreamReader;
    private readonly IAccountsImporter _accountsImporter;
    private readonly IIncomesImporter _incomesImporter;
    private readonly IExpensesImporter _expensesImporter;
    private readonly IMetadataSerializer _metadataSerializer;
    private readonly ILogger _logger;

    public ImportDataService(IImportStreamReaderFactory importStreamReader,
        IAccountsImporter accountImporter, IIncomesImporter incomesImporter, IExpensesImporter expenseImporter,
        IMetadataSerializer metadataSerializer, ILogger<ImportDataService> logger)
    {
        _importStreamReader = importStreamReader.WhenNotNull();
        _accountsImporter = accountImporter.WhenNotNull();
        _incomesImporter = incomesImporter.WhenNotNull();
        _expensesImporter = expenseImporter.WhenNotNull();
        _metadataSerializer = metadataSerializer.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        _ = zipStream.WhenNotNull();

        _logger.LogCall(this);

        var importReader = _importStreamReader.CreateReader(zipStream);

        if (!_expectedEntryNames.All(importReader.EntryNames.Contains))
        {
            var problemDetailsError = ProblemDetailsErrorFactory.CreateUnprocessableEntityError("The import file is not supported.");

            return EnrichedResult.Fail<int>(problemDetailsError);
        }

        MetadataBase metadataBase = GetMetadata(importReader);

        // There's only 1 version at the moment
        if (metadataBase.Version == 1)
        {
            var metadataV1 = (MetadataV1)metadataBase;
            _logger.LogInformation("Importing data (as v{MetadataVersion}) from {MetadataCreatedAt}", metadataV1.Version, metadataV1.CreatedAt);
        }

        var totalCount = 0;
        totalCount += await ImportAccountsAsync(importReader, cancellationToken);    // Must be first (incomes/expenses reference accounts)
        totalCount += await ImportIncomesAsync(importReader, cancellationToken);
        totalCount += await ImportExpensesAsync(importReader, cancellationToken);

        return EnrichedResult.Success(totalCount);
    }

    private MetadataBase GetMetadata(IImportStreamReader importReader)
    {
        using var stream = importReader.GetEntry("metadata");
        return _metadataSerializer.Deserialize(stream);
    }

    private Task<int> ImportAccountsAsync(IImportStreamReader importReader, CancellationToken cancellationToken)
    {
        return HandleEntry(importReader, "accounts", _accountsImporter.ImportAsync, cancellationToken);
    }

    private Task<int> ImportIncomesAsync(IImportStreamReader importReader, CancellationToken cancellationToken)
    {
        return HandleEntry(importReader, "incomes", _incomesImporter.ImportAsync, cancellationToken);
    }

    private Task<int> ImportExpensesAsync(IImportStreamReader importReader, CancellationToken cancellationToken)
    {
        return HandleEntry(importReader, "expenses", _expensesImporter.ImportAsync, cancellationToken);
    }

    private static async Task<int> HandleEntry(IImportStreamReader importReader, string name, Func<Stream, CancellationToken, Task<int>> handler,
        CancellationToken token)
    {
        using var stream = importReader.GetEntry(name);

        return await handler.Invoke(stream, token);
    }
}
