using AllOverIt.Assertion;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Pot.Shared.Enumerations;

namespace Pot.App.Concerns.Csv;

public class AccrualPolicyConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        _ = text.WhenNotNull();

        return AccrualPolicy.From(text);
    }
}
