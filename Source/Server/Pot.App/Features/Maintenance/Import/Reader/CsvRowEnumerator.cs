using AllOverIt.Assertion;
using CsvHelper;
using System.Collections;
using System.Globalization;

namespace Pot.App.Features.Maintenance.Import.Reader;

// This helper is responsible for enumerating the CSV rows from a stream while managing the lifecycle of the reader objects.
internal sealed class CsvRowEnumerator<TType, TAs> : ICsvRowEnumerator<TAs> where TType : TAs
{
    private bool _disposed;
    private StreamReader? _streamReader;

    // The stream is owned by this instance and will be disposed when enumeration is complete.
    public CsvRowEnumerator(Stream dataStream)
    {
        _ = dataStream.WhenNotNull();

        _streamReader = new(dataStream);
    }

    public IEnumerator<TAs> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var csv = new CsvReader(_streamReader!, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<TType>();

        foreach (var csvRow in csvRows)
        {
            yield return csvRow;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _streamReader?.Dispose();
            _streamReader = null;

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}