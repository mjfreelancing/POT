using Pot.AspNetCore.Concerns.RateLimiting.Configuration;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.RateLimiting.Configuration;

public class RateLimiterOptionsSetupFixture
{
    public class GetRetryAfterSeconds : RateLimiterOptionsSetupFixture
    {
        [Theory]
        [InlineData(59.873, 60)]
        [InlineData(60d, 60)]
        [InlineData(10.5, 11)]
        [InlineData(0.001, 1)]
        [InlineData(0d, 1)]
        [InlineData(-1d, 1)]
        public void Should_Return_A_Positive_Whole_Number_Of_Seconds(double totalSeconds, int expectedSeconds)
        {
            var result = RateLimiterOptionsSetup.GetRetryAfterSeconds(TimeSpan.FromSeconds(totalSeconds));

            result.ShouldBe(expectedSeconds);
        }
    }
}
