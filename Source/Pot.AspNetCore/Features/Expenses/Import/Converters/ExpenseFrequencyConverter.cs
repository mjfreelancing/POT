using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Data.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Converters
{
    public class ExpenseFrequencyConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            return ExpenseFrequency.From(text!);
        }
    }
}
