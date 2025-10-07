using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;
using System.Diagnostics;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Import.Reader;

internal sealed class ImportStreamReader : IImportStreamReader
{
    private readonly IMetadataSerializer _metadataSerializer;
    private readonly ILogger _logger;

    private ZipArchive? _archive;
    private Dictionary<string, ZipArchiveEntry>? _entries;
    private Lazy<string[]>? _entryNames;

    public string[] EntryNames => _entryNames?.Value ?? [];

    public ImportStreamReader(IMetadataSerializer metadataSerializer, ILogger<ImportStreamReader> logger)
    {
        _metadataSerializer = metadataSerializer.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public IDisposable Open(Stream stream)
    {
        Throw<UnreachableException>.WhenNotNull(_archive, "The import stream is already open");

        _ = stream.WhenNotNull();

        _archive = new(stream, ZipArchiveMode.Read);
        _entries = _archive.Entries.ToDictionary(kvp => kvp.Name);
        _entryNames = new Lazy<string[]>(() => [.. _entries.Keys]);

        return _archive;
    }

    public MetadataBase GetMetadata()
    {
        _logger.LogCall(this);

        using var stream = GetEntry("metadata");
        return _metadataSerializer.Deserialize(stream);
    }

    public ICsvRowEnumerator<IAccountCsvRow> GetAccounts()
    {
        _logger.LogCall(this);

        var dataStream = GetEntry("accounts");
        return new CsvRowEnumerator<AccountCsvRow, IAccountCsvRow>(dataStream);
    }

    public ICsvRowEnumerator<IExpenseCsvRow> GetExpenses()
    {
        _logger.LogCall(this);

        var dataStream = GetEntry("expenses");
        return new CsvRowEnumerator<ExpenseCsvRow, IExpenseCsvRow>(dataStream);
    }

    public ICsvRowEnumerator<IIncomeCsvRow> GetIncomes()
    {
        _logger.LogCall(this);

        var dataStream = GetEntry("incomes");
        return new CsvRowEnumerator<IncomeCsvRow, IIncomeCsvRow>(dataStream);
    }

    private Stream GetEntry(string name)
    {
        Throw<UnreachableException>.WhenNull(_entries, "The import stream has not been opened");

        // Will throw if the entry does not exist.
        // The caller must dispose the stream.
        return _entries[name].Open();
    }
}
