using AllOverIt.Assertion;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Import;

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
