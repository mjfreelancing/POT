using AllOverIt.Assertion;
using CsvHelper;
using System.Collections;
using System.Globalization;

namespace Pot.App.Features.Maintenance.Import.Reader;

// This helper is responsible for enumerating the CSV rows from a stream while managing the lifecycle of the reader objects.
// The underlying streams will be disposed only when iteration is complete.
internal sealed class CsvRowEnumerator<TType, TAs> : IEnumerable<TAs> where TType : TAs
{
    private readonly Stream _dataStream;

    public CsvRowEnumerator(Stream dataStream)
    {
        _dataStream = dataStream.WhenNotNull();
    }

    public IEnumerator<TAs> GetEnumerator()
    {
        using StreamReader reader = new(_dataStream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        var csvRows = csv.GetRecords<TType>();

        foreach (var csvRow in csvRows)
        {
            yield return csvRow;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}