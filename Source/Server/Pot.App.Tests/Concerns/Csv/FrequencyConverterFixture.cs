using AllOverIt.Fixture.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using NSubstitute;
using Pot.App.Concerns.Csv;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.App.Tests.Concerns.Csv;

public class FrequencyConverterFixture : PotFixtureBase
{
    public class ConvertFromString : FrequencyConverterFixture
    {
        private readonly FrequencyConverter _converter;
        private readonly IReaderRow _readerRow;
        private readonly MemberMapData _memberMapData;

        public ConvertFromString()
        {
            _converter = new FrequencyConverter();
            _readerRow = Substitute.For<IReaderRow>();
            _memberMapData = CreateMemberMapData();
        }

        [Fact]
        public void Should_Throw_When_Text_Null()
        {
            Invoking(() =>
            {
                _converter.ConvertFromString(null, _readerRow, _memberMapData);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("text");
        }

        [Fact]
        public void Should_Convert_Days_String_To_Days_Frequency()
        {
            var result = _converter.ConvertFromString("Days", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Days);
        }

        [Fact]
        public void Should_Convert_Weeks_String_To_Weeks_Frequency()
        {
            var result = _converter.ConvertFromString("Weeks", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Weeks);
        }

        [Fact]
        public void Should_Convert_Months_String_To_Months_Frequency()
        {
            var result = _converter.ConvertFromString("Months", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Months);
        }

        [Fact]
        public void Should_Convert_Years_String_To_Years_Frequency()
        {
            var result = _converter.ConvertFromString("Years", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Years);
        }

        [Fact]
        public void Should_Convert_OneTime_String_To_OneTime_Frequency()
        {
            var result = _converter.ConvertFromString("OneTime", _readerRow, _memberMapData);

            result.Should().Be(Frequency.OneTime);
        }

        [Fact]
        public void Should_Convert_Days_Integer_String_To_Days_Frequency()
        {
            var result = _converter.ConvertFromString("1", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Days);
        }

        [Fact]
        public void Should_Convert_Weeks_Integer_String_To_Weeks_Frequency()
        {
            var result = _converter.ConvertFromString("2", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Weeks);
        }

        [Fact]
        public void Should_Convert_Months_Integer_String_To_Months_Frequency()
        {
            var result = _converter.ConvertFromString("3", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Months);
        }

        [Fact]
        public void Should_Convert_Years_Integer_String_To_Years_Frequency()
        {
            var result = _converter.ConvertFromString("4", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Years);
        }

        [Fact]
        public void Should_Convert_OneTime_Integer_String_To_OneTime_Frequency()
        {
            var result = _converter.ConvertFromString("5", _readerRow, _memberMapData);

            result.Should().Be(Frequency.OneTime);
        }

        [Fact]
        public void Should_Be_Case_Insensitive_For_Name()
        {
            var result = _converter.ConvertFromString("days", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Days);
        }

        [Fact]
        public void Should_Convert_Uppercase_Name()
        {
            var result = _converter.ConvertFromString("WEEKS", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Weeks);
        }

        [Fact]
        public void Should_Convert_Mixed_Case_Name()
        {
            var result = _converter.ConvertFromString("MoNtHs", _readerRow, _memberMapData);

            result.Should().Be(Frequency.Months);
        }

        [Fact]
        public void Should_Throw_When_Invalid_Name()
        {
            Invoking(() =>
            {
                _converter.ConvertFromString("InvalidFrequency", _readerRow, _memberMapData);
            })
            .Should()
            .Throw<Exception>();
        }

        [Fact]
        public void Should_Throw_When_Invalid_Value()
        {
            Invoking(() =>
            {
                _converter.ConvertFromString("999", _readerRow, _memberMapData);
            })
            .Should()
            .Throw<Exception>();
        }

        [Fact]
        public void Should_Throw_When_Empty_String()
        {
            Invoking(() =>
            {
                _converter.ConvertFromString(string.Empty, _readerRow, _memberMapData);
            })
            .Should()
            .Throw<Exception>();
        }
    }

    public class ConvertToString : FrequencyConverterFixture
    {
        private readonly FrequencyConverter _converter;
        private readonly IWriterRow _writerRow;
        private readonly MemberMapData _memberMapData;

        public ConvertToString()
        {
            _converter = new FrequencyConverter();
            _writerRow = Substitute.For<IWriterRow>();
            _memberMapData = CreateMemberMapData();
        }

        [Fact]
        public void Should_Convert_Days_Frequency_To_Days_String()
        {
            var result = _converter.ConvertToString(Frequency.Days, _writerRow, _memberMapData);

            result.Should().Be("Days");
        }

        [Fact]
        public void Should_Convert_Weeks_Frequency_To_Weeks_String()
        {
            var result = _converter.ConvertToString(Frequency.Weeks, _writerRow, _memberMapData);

            result.Should().Be("Weeks");
        }

        [Fact]
        public void Should_Convert_Months_Frequency_To_Months_String()
        {
            var result = _converter.ConvertToString(Frequency.Months, _writerRow, _memberMapData);

            result.Should().Be("Months");
        }

        [Fact]
        public void Should_Convert_Years_Frequency_To_Years_String()
        {
            var result = _converter.ConvertToString(Frequency.Years, _writerRow, _memberMapData);

            result.Should().Be("Years");
        }

        [Fact]
        public void Should_Convert_OneTime_Frequency_To_OneTime_String()
        {
            var result = _converter.ConvertToString(Frequency.OneTime, _writerRow, _memberMapData);

            result.Should().Be("OneTime");
        }

        [Fact]
        public void Should_Handle_Null_Value()
        {
            var result = _converter.ConvertToString(null, _writerRow, _memberMapData);

            result.Should().Be(string.Empty);
        }
    }

    private static MemberMapData CreateMemberMapData()
    {
        var typeConverterOptions = new CsvHelper.TypeConversion.TypeConverterOptions();

        var memberMapData = new MemberMapData(null!)
        {
            TypeConverterOptions = typeConverterOptions
        };

        return memberMapData;
    }
}
