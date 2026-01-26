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

        var formatProvider = GetFormatProvider(memberMapData);
        var dateTimeStyle = GetDateTimeStyle(memberMapData);

        return ParseDateOnly(text, memberMapData.TypeConverterOptions.Formats, formatProvider, dateTimeStyle);
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var dateOnly = (DateOnly)value;
        var formatProvider = GetFormatProvider(memberMapData);
        var format = GetFirstFormat(memberMapData);

        return format is not null
            ? dateOnly.ToString(format, formatProvider)
            : dateOnly.ToString(formatProvider);
    }

    private static IFormatProvider? GetFormatProvider(MemberMapData memberMapData)
    {
        var cultureInfo = memberMapData.TypeConverterOptions.CultureInfo;

        return (IFormatProvider?)cultureInfo?.GetFormat(typeof(DateTimeFormatInfo)) ?? cultureInfo;
    }

    private static DateTimeStyles GetDateTimeStyle(MemberMapData memberMapData)
    {
        return memberMapData.TypeConverterOptions.DateTimeStyle ?? DateTimeStyles.None;
    }

    private static DateOnly ParseDateOnly(string text, string[]? formats, IFormatProvider? formatProvider, DateTimeStyles dateTimeStyle)
    {
        if (formats is null || formats.Length == 0)
        {
            return DateOnly.Parse(text, formatProvider, dateTimeStyle);
        }

        return DateOnly.ParseExact(text, formats, formatProvider, dateTimeStyle);
    }

    private static string? GetFirstFormat(MemberMapData memberMapData)
    {
        var formats = memberMapData.TypeConverterOptions.Formats;

        return formats is { Length: > 0 } ? formats[0] : null;
    }
}
