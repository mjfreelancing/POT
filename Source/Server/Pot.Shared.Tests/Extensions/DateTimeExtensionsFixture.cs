using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Shared.Tests.Extensions;

public class DateTimeExtensionsFixture : PotFixtureBase
{
    public class GetEtag : DateTimeExtensionsFixture
    {
        [Fact]
        public void Should_Return_Unix_Milliseconds_For_Utc_DateTime()
        {
            var dateTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

            var result = dateTime.GetEtag();

            result.ShouldBe(expected);
        }

        [Fact]
        public void Should_Convert_Local_DateTime_To_Utc_Before_Computing_Etag()
        {
            var localDateTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Local);
            var utcEquivalent = localDateTime.ToUniversalTime();
            var expected = new DateTimeOffset(utcEquivalent).ToUnixTimeMilliseconds();

            var result = localDateTime.GetEtag();

            result.ShouldBe(expected);
        }
    }
}
