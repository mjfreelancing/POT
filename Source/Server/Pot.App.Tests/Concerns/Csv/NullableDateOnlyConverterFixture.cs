using CsvHelper;
using CsvHelper.Configuration;
using Shouldly;
using NSubstitute;
using Pot.App.Concerns.Csv;
using Pot.TestUtils;
using System.Globalization;

namespace Pot.App.Tests.Concerns.Csv;

public class NullableDateOnlyConverterFixture : PotFixtureBase
{
    public class ConvertFromString : NullableDateOnlyConverterFixture
    {
        private readonly NullableDateOnlyConverter _converter;
        private readonly IReaderRow _readerRow;

        public ConvertFromString()
        {
            _converter = new NullableDateOnlyConverter();
            _readerRow = Substitute.For<IReaderRow>();
        }

        [Fact]
        public void Should_Return_Null_When_Text_Is_Null()
        {
            var memberMapData = CreateMemberMapData();

            var result = _converter.ConvertFromString(null, _readerRow, memberMapData);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_Return_Null_When_Text_Is_Empty()
        {
            var memberMapData = CreateMemberMapData();

            var result = _converter.ConvertFromString(string.Empty, _readerRow, memberMapData);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_Return_Null_When_Text_Is_Whitespace()
        {
            var memberMapData = CreateMemberMapData();

            var result = _converter.ConvertFromString("   ", _readerRow, memberMapData);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_Parse_Date_Using_Default_Format_When_No_Formats_Specified()
        {
            var memberMapData = CreateMemberMapData();
            var expectedDate = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertFromString("12/25/2024", _readerRow, memberMapData);

            result.ShouldBe(expectedDate);
        }

        [Fact]
        public void Should_Parse_Date_Using_Specific_Format_When_Format_Specified()
        {
            var memberMapData = CreateMemberMapData(formats: ["yyyy-MM-dd"]);
            var expectedDate = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertFromString("2024-12-25", _readerRow, memberMapData);

            result.ShouldBe(expectedDate);
        }

        [Fact]
        public void Should_Parse_Date_Using_Multiple_Formats_When_Multiple_Formats_Specified()
        {
            var memberMapData = CreateMemberMapData(formats: ["yyyy-MM-dd", "dd/MM/yyyy"]);
            var expectedDate = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertFromString("25/12/2024", _readerRow, memberMapData);

            result.ShouldBe(expectedDate);
        }

        [Fact]
        public void Should_Parse_Date_Using_Culture_Info_When_Specified()
        {
            var memberMapData = CreateMemberMapData(cultureInfo: CultureInfo.GetCultureInfo("en-GB"));
            var expectedDate = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertFromString("25/12/2024", _readerRow, memberMapData);

            result.ShouldBe(expectedDate);
        }

        [Fact]
        public void Should_Parse_Date_Using_DateTimeStyle_When_Specified()
        {
            var memberMapData = CreateMemberMapData(
                formats: ["yyyy-MM-dd"],
                dateTimeStyle: DateTimeStyles.AllowWhiteSpaces
            );
            var expectedDate = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertFromString("  2024-12-25  ", _readerRow, memberMapData);

            result.ShouldBe(expectedDate);
        }

        [Fact]
        public void Should_Throw_FormatException_When_Date_Is_Invalid()
        {
            var memberMapData = CreateMemberMapData();

            Should.Throw<FormatException>(() => {
                _converter.ConvertFromString("invalid-date", _readerRow, memberMapData);
            });
        }
    }

    public class ConvertToString : NullableDateOnlyConverterFixture
    {
        private readonly NullableDateOnlyConverter _converter;
        private readonly IWriterRow _writerRow;

        public ConvertToString()
        {
            _converter = new NullableDateOnlyConverter();
            _writerRow = Substitute.For<IWriterRow>();
        }

        [Fact]
        public void Should_Return_Empty_String_When_Value_Is_Null()
        {
            var memberMapData = CreateMemberMapData();

            var result = _converter.ConvertToString(null, _writerRow, memberMapData);

            result.ShouldBe(string.Empty);
        }

        [Fact]
        public void Should_Format_Date_Using_Default_Format_When_No_Formats_Specified()
        {
            var memberMapData = CreateMemberMapData();
            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("12/25/2024");
        }

        [Fact]
        public void Should_Format_Date_Using_Specific_Format_When_Format_Specified()
        {
            var memberMapData = CreateMemberMapData(formats: ["yyyy-MM-dd"]);
            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("2024-12-25");
        }

        [Fact]
        public void Should_Use_First_Format_When_Multiple_Formats_Specified()
        {
            var memberMapData = CreateMemberMapData(formats: ["yyyy-MM-dd", "dd/MM/yyyy"]);
            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("2024-12-25");
        }

        [Fact]
        public void Should_Format_Date_Using_Culture_Info_When_Specified()
        {
            var memberMapData = CreateMemberMapData(cultureInfo: CultureInfo.GetCultureInfo("en-GB"));
            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("25/12/2024");
        }

        [Fact]
        public void Should_Format_Date_With_Custom_Format_And_Culture()
        {
            var memberMapData = CreateMemberMapData(
                formats: ["dd MMM yyyy"],
                cultureInfo: CultureInfo.GetCultureInfo("en-US")
            );

            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("25 Dec 2024");
        }

        [Fact]
        public void Should_Handle_Empty_Format_Array_As_No_Format()
        {
            var memberMapData = CreateMemberMapData(formats: []);
            var date = new DateOnly(2024, 12, 25);

            var result = _converter.ConvertToString(date, _writerRow, memberMapData);

            result.ShouldBe("12/25/2024");
        }
    }

    private static MemberMapData CreateMemberMapData(string[]? formats = null, CultureInfo? cultureInfo = null,
        DateTimeStyles? dateTimeStyle = null)
    {
        var typeConverterOptions = new CsvHelper.TypeConversion.TypeConverterOptions
        {
            Formats = formats,
            CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture,
            DateTimeStyle = dateTimeStyle
        };

        var memberMapData = new MemberMapData(null!)
        {
            TypeConverterOptions = typeConverterOptions
        };

        return memberMapData;
    }
}
