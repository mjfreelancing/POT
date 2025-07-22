using AllOverIt.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace Pot.App.Concerns.Csv;

// Will use the format style specified in the [Format] attribute on the property,for example:
// [Format("yyyy-MM-dd")]
public class NullableDateOnlyConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text.IsNullOrEmpty())
        {
            return null;
        }

        var formatProvider = (IFormatProvider?)memberMapData.TypeConverterOptions.CultureInfo?.GetFormat(typeof(DateTimeFormatInfo)) ?? memberMapData.TypeConverterOptions.CultureInfo;
        var dateTimeStyle = memberMapData.TypeConverterOptions.DateTimeStyle ?? DateTimeStyles.None;

        return memberMapData.TypeConverterOptions.Formats == null || memberMapData.TypeConverterOptions.Formats.Length == 0
            ? DateOnly.Parse(text, formatProvider, dateTimeStyle)
            : DateOnly.ParseExact(text, memberMapData.TypeConverterOptions.Formats, formatProvider, dateTimeStyle);
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var formatProvider = (IFormatProvider?)memberMapData.TypeConverterOptions.CultureInfo?.GetFormat(typeof(DateTimeFormatInfo)) ?? memberMapData.TypeConverterOptions.CultureInfo;

        if (memberMapData.TypeConverterOptions.Formats?.Length > 0)
        {
            return ((DateOnly)value).ToString(memberMapData.TypeConverterOptions.Formats[0], formatProvider);
        }

        return ((DateOnly)value).ToString(formatProvider);
    }
}
