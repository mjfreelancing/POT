using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Reader;

public interface IImportStreamReader : IPotScopedDependency
{
    string[] EntryNames { get; }

    IDisposable Open(Stream stream);
    int ReadMetadataVersion();
    MetadataBase GetMetadata();

    // Note: ICsvRowEnumerator<T> is IDisposable
    ICsvRowEnumerator<IAccountCsvRow> GetAccounts();
    ICsvRowEnumerator<IExpenseCsvRow> GetExpenses();
    ICsvRowEnumerator<IIncomeCsvRow> GetIncomes();
}
