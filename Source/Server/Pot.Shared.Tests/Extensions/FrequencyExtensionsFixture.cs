using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Shared.Tests.Extensions;

public class FrequencyExtensionsFixture : PotFixtureBase
{
    public class GetDaysToNext : FrequencyExtensionsFixture
    {
        [Fact]
        public void Should_Return_Frequency_Count_For_Days()
        {
            var result = Frequency.Days.GetDaysToNext(new DateOnly(2026, 3, 24), 5);

            result.ShouldBe(5);
        }

        [Fact]
        public void Should_Return_Seven_Times_Frequency_Count_For_Weeks()
        {
            var result = Frequency.Weeks.GetDaysToNext(new DateOnly(2026, 3, 24), 3);

            result.ShouldBe(21);
        }

        [Fact]
        public void Should_Return_Calendar_Days_For_Months()
        {
            var result = Frequency.Months.GetDaysToNext(new DateOnly(2026, 1, 31), 1);

            result.ShouldBe(28);
        }

        [Fact]
        public void Should_Return_Calendar_Days_For_Years()
        {
            var result = Frequency.Years.GetDaysToNext(new DateOnly(2024, 2, 29), 1);

            result.ShouldBe(365);
        }

        [Fact]
        public void Should_Throw_For_OneTime()
        {
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                _ = Frequency.OneTime.GetDaysToNext(new DateOnly(2026, 3, 24), 1);
            });

            exception.Message.ShouldBe("The 'OneTime' frequency does not have a next occurrence.");
        }

        [Fact]
        public void Should_Throw_For_Null_Frequency()
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                _ = FrequencyExtensions.GetDaysToNext(null!, new DateOnly(2026, 3, 24), 1);
            });

            exception.ParamName.ShouldBe("frequency");
        }
    }

    public class GetAverageDaysToNext : FrequencyExtensionsFixture
    {
        [Fact]
        public void Should_Return_Frequency_Count_For_Days()
        {
            var result = Frequency.Days.GetAverageDaysToNext(5);

            result.ShouldBe(5d);
        }

        [Fact]
        public void Should_Return_Seven_Times_Frequency_Count_For_Weeks()
        {
            var result = Frequency.Weeks.GetAverageDaysToNext(3);

            result.ShouldBe(21d);
        }

        [Fact]
        public void Should_Return_Average_Month_Length_Times_Frequency_Count_For_Months()
        {
            var result = Frequency.Months.GetAverageDaysToNext(2);

            result.ShouldBe((365.2425d / 12d) * 2d, 0.0000000001d);
        }

        [Fact]
        public void Should_Return_Average_Year_Length_Times_Frequency_Count_For_Years()
        {
            var result = Frequency.Years.GetAverageDaysToNext(4);

            result.ShouldBe(365.2425d * 4d, 0.0000000001d);
        }

        [Fact]
        public void Should_Throw_For_OneTime()
        {
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                _ = Frequency.OneTime.GetAverageDaysToNext(1);
            });

            exception.Message.ShouldBe($"The '{Frequency.OneTime.Name}' frequency does not have a recurring average period.");
        }

        [Fact]
        public void Should_Throw_For_Null_Frequency()
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                _ = FrequencyExtensions.GetAverageDaysToNext(null!, 1);
            });

            exception.ParamName.ShouldBe("frequency");
        }
    }
}