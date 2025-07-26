using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Zip;
using Pot.App.Concerns.Zip;
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

    public ExportDataService(IAccountsExporter accountExporter, IIncomesExporter incomesExporter, IExpensesExporter expensesExporter,
        IMetadataSerializer metadataSerializer, IZipPackageFactory zipPackageFactory)
    {
        _accountsExporter = accountExporter.WhenNotNull();
        _incomesExporter = incomesExporter.WhenNotNull();
        _expensesExporter = expensesExporter.WhenNotNull();
        _metadataSerializer = metadataSerializer.WhenNotNull();
        _zipPackageFactory = zipPackageFactory.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        using var zipPackage = _zipPackageFactory.CreateZipPackage();

        await AddToZipAsync(zipPackage, "metadata", ExportMetadataAsync, cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "accounts", _accountsExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "incomes", _incomesExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);
        await AddToZipAsync(zipPackage, "expenses", _expensesExporter.ExportAllAsync, cancellationToken).ConfigureAwait(false);

        zipPackage.Complete();

        return zipPackage.Content.ToByteArray();
    }

    private static async Task AddToZipAsync(IZipPackage zipPackage, string entryName, Func<CancellationToken,
        Task<byte[]>> contentResolver, CancellationToken cancellationToken)
    {
        var content = await contentResolver.Invoke(cancellationToken).ConfigureAwait(false);

        await zipPackage
            .AddEntryAsync(entryName, content.AsMemory(), cancellationToken)
            .ConfigureAwait(false);
    }

    private Task<byte[]> ExportMetadataAsync(CancellationToken _)
    {
        var metadata = new MetadataV1
        {
            CreatedAt = DateTime.UtcNow
        };

        var bytes = _metadataSerializer.Serialize(metadata);

        return Task.FromResult(bytes);
    }
}
