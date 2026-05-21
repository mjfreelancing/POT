using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Shared.Tests.Extensions;

public class DateOnlyExtensionsFixture : PotFixtureBase
{
    public class DaysUntil : DateOnlyExtensionsFixture
    {
        [Fact]
        public void Should_Return_Zero_When_Start_And_End_Dates_Are_The_Same()
        {
            var date = new DateOnly(2025, 6, 15);

            var result = date.DaysUntil(date);

            result.ShouldBe(0);
        }

        [Fact]
        public void Should_Return_Positive_Count_When_End_Date_Is_After_Start_Date()
        {
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2025, 1, 31);

            var result = startDate.DaysUntil(endDate);

            result.ShouldBe(30);
        }

        [Fact]
        public void Should_Return_Negative_Count_When_End_Date_Is_Before_Start_Date()
        {
            var startDate = new DateOnly(2025, 1, 31);
            var endDate = new DateOnly(2025, 1, 1);

            var result = startDate.DaysUntil(endDate);

            result.ShouldBe(-30);
        }
    }
}
