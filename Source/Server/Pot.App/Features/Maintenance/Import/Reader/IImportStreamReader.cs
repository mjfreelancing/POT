using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Reader;

public interface IImportStreamReader : IPotScopedDependency
{
    string[] EntryNames { get; }

    IDisposable Open(Stream stream);
    int ReadMetadataVersion();

    // Forcing the caller to specify the expected metadata type since they must know the expected type
    // of metadata to read. The version is read separately to allow for version-specific handling
    // before reading the metadata.
    TMetadata GetMetadata<TMetadata>() where TMetadata : MetadataBase;

    // Note: ICsvRowEnumerator<T> is IDisposable
    ICsvRowEnumerator<IAccountCsvRow> GetAccounts();
    ICsvRowEnumerator<IExpenseCsvRow> GetExpenses();
    ICsvRowEnumerator<IIncomeCsvRow> GetIncomes();
}
