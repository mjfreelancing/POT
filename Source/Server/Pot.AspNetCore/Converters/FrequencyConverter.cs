using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Data.Models;

namespace Pot.AspNetCore.Converters
{
    public class FrequencyConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            return Frequency.From(text!);
        }
    }
}
