using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;
using Xunit;

namespace Pot.Shared.Tests.Extensions;

public class FrequencyExtensionsFixture : PotFixtureBase
{
    public class GetDaysToNext : FrequencyExtensionsFixture
    {
        public static IEnumerable<object[]> EndOfMonthMonthlyTraversalCases()
        {
            yield return
            [
                new DateOnly(2023, 12, 31),
                new[]
                {
                    new DateOnly(2024, 1, 31),
                    new DateOnly(2024, 2, 29),
                    new DateOnly(2024, 3, 31),
                    new DateOnly(2024, 4, 30),
                    new DateOnly(2024, 5, 31),
                    new DateOnly(2024, 6, 30),
                    new DateOnly(2024, 7, 31),
                    new DateOnly(2024, 8, 31),
                    new DateOnly(2024, 9, 30),
                    new DateOnly(2024, 10, 31),
                    new DateOnly(2024, 11, 30),
                    new DateOnly(2024, 12, 31),
                    new DateOnly(2025, 1, 31),
                    new DateOnly(2025, 2, 28),
                    new DateOnly(2025, 3, 31),
                },
            ];
        }

        public static IEnumerable<object[]> EndOfMonthQuarterlyTraversalCases()
        {
            yield return
            [
                new DateOnly(2023, 12, 31),
                new[]
                {
                    new DateOnly(2024, 3, 31),
                    new DateOnly(2024, 6, 30),
                    new DateOnly(2024, 9, 30),
                    new DateOnly(2024, 12, 31),
                    new DateOnly(2025, 3, 31),
                    new DateOnly(2025, 6, 30),
                },
            ];
        }

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
        public void Should_Return_Days_To_End_Of_Target_Month_For_EndOfMonth()
        {
            var result = Frequency.EndOfMonth.GetDaysToNext(new DateOnly(2025, 1, 31), 1);

            result.ShouldBe(28);
        }

        [Fact]
        public void Should_Return_Days_To_End_Of_Target_Month_For_EndOfMonth_With_FrequencyCount_Greater_Than_One()
        {
            var result = Frequency.EndOfMonth.GetDaysToNext(new DateOnly(2025, 1, 31), 2);

            result.ShouldBe(59);
        }

        [Theory]
        [MemberData(nameof(EndOfMonthMonthlyTraversalCases))]
        public void Should_Advance_EndOfMonth_One_Month_At_A_Time_Through_Leap_And_NonLeap_Februaries(
            DateOnly startDate,
            IReadOnlyList<DateOnly> expectedDates)
        {
            var currentDate = startDate;

            foreach (var expectedDate in expectedDates)
            {
                var daysToNext = Frequency.EndOfMonth.GetDaysToNext(currentDate, 1);

                currentDate = currentDate.AddDays(daysToNext);

                currentDate.ShouldBe(expectedDate);
            }
        }

        [Theory]
        [MemberData(nameof(EndOfMonthQuarterlyTraversalCases))]
        public void Should_Advance_EndOfMonth_Three_Months_At_A_Time(
            DateOnly startDate,
            IReadOnlyList<DateOnly> expectedDates)
        {
            var currentDate = startDate;

            foreach (var expectedDate in expectedDates)
            {
                var daysToNext = Frequency.EndOfMonth.GetDaysToNext(currentDate, 3);

                currentDate = currentDate.AddDays(daysToNext);

                currentDate.ShouldBe(expectedDate);
            }
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
        public void Should_Return_Average_Month_Length_Times_Frequency_Count_For_EndOfMonth()
        {
            var result = Frequency.EndOfMonth.GetAverageDaysToNext(2);

            result.ShouldBe((365.2425d / 12d) * 2d, 0.0000000001d);
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