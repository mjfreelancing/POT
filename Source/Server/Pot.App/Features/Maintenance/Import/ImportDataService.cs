using AllOverIt.Assertion;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Maintenance.Import.Accounts;
using Pot.App.Features.Maintenance.Import.Expenses;
using Pot.App.Features.Maintenance.Import.Incomes;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Import;

internal sealed class ImportDataService : IImportDataService
{
    private readonly string[] _expectedEntryNames = { "metadata", "accounts", "incomes", "expenses" };
    private readonly IAccountsImporter _accountsImporter;
    private readonly IIncomesImporter _incomesImporter;
    private readonly IExpensesImporter _expensesImporter;
    private readonly IMetadataSerializer _metadataSerializer;
    private readonly ILogger _logger;

    public ImportDataService(IAccountsImporter accountImporter, IIncomesImporter incomesImporter, IExpensesImporter expenseImporter,
        IMetadataSerializer metadataSerializer, ILogger<ImportDataService> logger)
    {
        _accountsImporter = accountImporter.WhenNotNull();
        _incomesImporter = incomesImporter.WhenNotNull();
        _expensesImporter = expenseImporter.WhenNotNull();
        _metadataSerializer = metadataSerializer.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<int>> ImportAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        _ = zipStream.WhenNotNull();

        using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);

        var entries = archive.Entries.ToDictionary(kvp => kvp.Name);

        if (!_expectedEntryNames.All(entries.ContainsKey))
        {
            var problemDetailsError = ProblemDetailsErrorFactory.CreateUnprocessableEntityError("The import file is not supported.");

            return EnrichedResult.Fail<int>(problemDetailsError);
        }

        MetadataBase metadataBase = GetMetadata(entries["metadata"]);

        // There's only 1 version at the moment
        if (metadataBase.Version == 1)
        {
            var metadataV1 = (MetadataV1)metadataBase;
            _logger.LogInformation("Importing data (as v{MetadataVersion}) from {MetadataCreatedAt}", metadataV1.Version, metadataV1.CreatedAt);
        }

        var totalCount = 0;
        totalCount += await ImportAccountsAsync(entries["accounts"], cancellationToken);    // Must be first
        totalCount += await ImportIncomesAsync(entries["incomes"], cancellationToken);
        totalCount += await ImportExpensesAsync(entries["expenses"], cancellationToken);

        return EnrichedResult.Success(totalCount);
    }

    private MetadataBase GetMetadata(ZipArchiveEntry zipArchiveEntry)
    {
        using var stream = zipArchiveEntry.Open();
        return _metadataSerializer.Deserialize(stream);
    }

    private Task<int> ImportAccountsAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        return HandleEntry(entry, _accountsImporter.ImportAsync, cancellationToken);
    }

    private Task<int> ImportIncomesAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        return HandleEntry(entry, _incomesImporter.ImportAsync, cancellationToken);
    }

    private Task<int> ImportExpensesAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        return HandleEntry(entry, _expensesImporter.ImportAsync, cancellationToken);
    }

    private static async Task<int> HandleEntry(ZipArchiveEntry entry, Func<Stream, CancellationToken, Task<int>> handler, CancellationToken token)
    {
        using var stream = entry.Open();
        return await handler.Invoke(stream, token);
    }
}
