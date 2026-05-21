using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Shared.Tests.Extensions;

public class AccrualPolicyExtensionsFixture : PotFixtureBase
{
    public class GetCanonicalAccrualStart : AccrualPolicyExtensionsFixture
    {
        [Fact]
        public void Should_Return_Null_When_Policy_Is_None_And_AccrualStart_Is_Null()
        {
            var automaticDefaultStart = new DateOnly(2025, 1, 1);

            var result = AccrualPolicy.None.GetCanonicalAccrualStart(null, automaticDefaultStart);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_Return_Null_When_Policy_Is_None_And_AccrualStart_Is_Non_Null()
        {
            var accrualStart = new DateOnly(2024, 6, 15);
            var automaticDefaultStart = new DateOnly(2025, 1, 1);

            var result = AccrualPolicy.None.GetCanonicalAccrualStart(accrualStart, automaticDefaultStart);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_Return_AccrualStart_When_Policy_Is_Automatic_And_AccrualStart_Is_Non_Null()
        {
            var accrualStart = new DateOnly(2024, 6, 15);
            var automaticDefaultStart = new DateOnly(2025, 1, 1);

            var result = AccrualPolicy.Automatic.GetCanonicalAccrualStart(accrualStart, automaticDefaultStart);

            result.ShouldBe(accrualStart);
        }

        [Fact]
        public void Should_Return_AutomaticDefaultStart_When_Policy_Is_Automatic_And_AccrualStart_Is_Null()
        {
            var automaticDefaultStart = new DateOnly(2025, 1, 1);

            var result = AccrualPolicy.Automatic.GetCanonicalAccrualStart(null, automaticDefaultStart);

            result.ShouldBe(automaticDefaultStart);
        }
    }
}
