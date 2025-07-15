using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Zip;
using Pot.App.Concerns.Zip;

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
