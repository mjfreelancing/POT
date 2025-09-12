using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Zip;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Zip;
using Pot.App.Features.Maintenance.Encryptor;
using Pot.App.Features.Maintenance.Export.Accounts;
using Pot.App.Features.Maintenance.Export.Expenses;
using Pot.App.Features.Maintenance.Export.Incomes;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Export;

internal sealed class ExportDataService : IExportDataService
{
    private readonly IAccountsExporter _accountsExporter;
    private readonly IIncomesExporter _incomesExporter;
    private readonly IExpensesExporter _expensesExporter;
    private readonly IMetadataSerializer _metadataSerializer;
    private readonly IZipPackageFactory _zipPackageFactory;
    private readonly IExportEncryptor _exportEncryptor;
    private readonly ILogger _logger;

    public ExportDataService(IAccountsExporter accountExporter, IIncomesExporter incomesExporter, IExpensesExporter expensesExporter,
        IMetadataSerializer metadataSerializer, IZipPackageFactory zipPackageFactory, IExportEncryptor exportEncryptor,
        ILogger<ExportDataService> logger)
    {
        _accountsExporter = accountExporter.WhenNotNull();
        _incomesExporter = incomesExporter.WhenNotNull();
        _expensesExporter = expensesExporter.WhenNotNull();
        _metadataSerializer = metadataSerializer.WhenNotNull();
        _zipPackageFactory = zipPackageFactory.WhenNotNull();
        _exportEncryptor = exportEncryptor.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(string publicKey, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using var zipPackage = _zipPackageFactory.CreateZipPackage();

        // TODO: The import currently assigns the site associated with the user performing the import. Need to update the export/import to consider how this should all now work.

        await AddToZipAsync(zipPackage, "metadata", false, publicKey, (token) => ExportMetadataAsync(), cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "accounts", true, publicKey, _accountsExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "incomes", true, publicKey, _incomesExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "expenses", true, publicKey, _expensesExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);

        zipPackage.Complete();

        return zipPackage.Content.ToByteArray();
    }

    private async Task AddToZipAsync(IZipPackage zipPackage, string entryName, bool encrypt, string publicKey,
        Func<CancellationToken, Task<byte[]>> contentResolver, CancellationToken cancellationToken)
    {
        var content = await contentResolver.Invoke(cancellationToken).ConfigureAwait(false);
        var zipContent = encrypt ? _exportEncryptor.Encrypt(publicKey, content) : content;

        await zipPackage
            .AddEntryAsync(entryName, zipContent, cancellationToken)
            .ConfigureAwait(false);
    }

    private Task<byte[]> ExportMetadataAsync()
    {
        var metadata = new MetadataV1
        {
            CreatedAt = DateTime.UtcNow
        };

        var bytes = _metadataSerializer.Serialize(metadata);

        return Task.FromResult(bytes);
    }
}
