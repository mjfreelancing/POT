using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Shared;

namespace Pot.AspNetCore.Concerns.Converters.Csv
{
    public class FrequencyConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            return Frequency.From(text!);
        }
    }
}
