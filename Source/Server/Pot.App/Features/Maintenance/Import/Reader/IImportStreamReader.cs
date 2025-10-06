using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Reader;

public interface IImportStreamReader : IPotScopedDependency
{
    string[] EntryNames { get; }

    IDisposable Open(Stream stream);

    MetadataBase GetMetadata();
    IEnumerable<IAccountCsvRow> GetAccounts();
    IEnumerable<IExpenseCsvRow> GetExpenses();
    IEnumerable<IIncomeCsvRow> GetIncomes();
}
