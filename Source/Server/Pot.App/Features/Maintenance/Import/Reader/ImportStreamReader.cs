using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Pot.App.Features.Maintenance.Import.Reader;

internal sealed class ImportStreamReader : IImportStreamReader
{
    private bool _disposed;
    private ZipArchive? _archive;
    private readonly Dictionary<string, ZipArchiveEntry> _entries;
    private readonly Lazy<string[]> _entryNames;
    private readonly ILogger _logger;

    public string[] EntryNames => _entryNames.Value;

    public ImportStreamReader(Stream stream, ILogger logger)
    {
        _ = stream.WhenNotNull();
        _ = logger.WhenNotNull();

        _archive = new(stream, ZipArchiveMode.Read);
        _entries = _archive.Entries.ToDictionary(kvp => kvp.Name);
        _entryNames = new Lazy<string[]>(() => [.. _entries.Keys]);
        _logger = logger;
    }

    public Stream GetEntry(string name)
    {
        _logger.LogCall(this, new { name });

        // Will throw if the entry does not exist.
        // The caller must dispose the stream.
        return _entries[name].Open();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _archive is not null)
            {
                _archive.Dispose();
                _archive = null;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
