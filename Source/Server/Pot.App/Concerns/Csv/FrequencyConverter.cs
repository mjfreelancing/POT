using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Shared;

namespace Pot.App.Concerns.Csv;

public class FrequencyConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return Frequency.From(text!);
    }
}
