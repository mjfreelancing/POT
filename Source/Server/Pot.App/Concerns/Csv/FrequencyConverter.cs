using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Shared.Enumerations;

namespace Pot.App.Concerns.Csv;

public class FrequencyConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));

        return Frequency.From(text);
    }
}
