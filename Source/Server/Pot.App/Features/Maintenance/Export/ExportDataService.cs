using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Pot.Shared.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Export;

internal sealed class ExportDataService : IExportDataService
{
    private readonly IAccountsExporter _accountsExporter;
    private readonly IIncomesExporter _incomesExporter;
    private readonly IExpensesExporter _expensesExporter;
    private readonly IZipPackageFactory _zipPackageFactory;

    public ExportDataService(IAccountsExporter accountExporter, IIncomesExporter incomesExporter, IExpensesExporter expensesExporter,
        IZipPackageFactory zipPackageFactory)
    {
        _accountsExporter = accountExporter.WhenNotNull();
        _incomesExporter = incomesExporter.WhenNotNull();
        _expensesExporter = expensesExporter.WhenNotNull();
        _zipPackageFactory = zipPackageFactory.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        using var zipPackage = _zipPackageFactory.CreateZipPackage();

        // Not running in parallel until there is a performance concern. Would require a unique DbContext for each repository.
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
}


public interface IZipPackageFactory : IPotScopedDependency
{
    IZipPackage CreateZipPackage();
}

internal sealed class ZipPackageFactory : IZipPackageFactory
{
    public IZipPackage CreateZipPackage()
    {
        return new ZipPackage();
    }
}

public interface IZipPackage : IPotScopedDependency, IDisposable
{
    // Complete() must be called to access the Content stream
    Stream Content { get; }

    Task AddEntryAsync(string entryName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken);
    void Complete();
}

internal sealed class ZipPackage : IZipPackage
{
    private MemoryStream? _memoryStream;
    private ZipArchive? _archive;

    public Stream Content => GetArchiveStream();

    public ZipPackage()
    {
        _memoryStream = new MemoryStream();
        _archive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, true);
    }

    public async Task AddEntryAsync(string entryName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        _ = entryName.WhenNotNullOrEmpty();

        Throw<InvalidOperationException>.WhenNull(_archive, "The archive has already been disposed.");

        cancellationToken.ThrowIfCancellationRequested();

        var zipEntry = _archive.CreateEntry(entryName, CompressionLevel.Optimal);

        using var zipStream = zipEntry.Open();

        await zipStream.WriteAsync(content, cancellationToken).ConfigureAwait(false);
    }

    public void Complete()
    {
        Throw<InvalidOperationException>.WhenNull(_memoryStream, "The archive has already been completed.");

        // The archive must be closed to ensure the stream is completely written to
        DisposeArchive();

        _memoryStream.Position = 0;
    }

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        DisposeArchive();

        _memoryStream?.Dispose();
        _memoryStream = null;
    }

    private void DisposeArchive()
    {
        _archive?.Dispose();
        _archive = null;
    }

    private MemoryStream GetArchiveStream()
    {
        Throw<InvalidOperationException>.WhenNotNull(_archive, "The archive must be completed to access the stream.");

        return _memoryStream!;
    }
}
