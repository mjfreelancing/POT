using Pot.App.Calculators;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Calculators;

public class MonetaryRoundingFixture : PotFixtureBase
{
    [Fact]
    public void Should_Round_To_Two_Decimals_Away_From_Zero()
    {
        // A double holding 1.005 holds a little less than that, so rounding it directly would answer 1.00.
        MonetaryRounding.Round(1.005d).ShouldBe(1.01d);
        MonetaryRounding.Round(-1.005d).ShouldBe(-1.01d);
    }

    [Fact]
    public void Should_Publish_Zero_For_A_Negative_Value_That_Rounds_To_Nothing()
    {
        var unrounded = 0.3d - 0.1d - 0.2d;

        // The value being rounded is genuinely below zero and not representable as zero, so the rounding step is
        // what produces the zero, and the sign of what it produces is the thing under test.
        unrounded.ShouldNotBe(0.0d);
        double.IsNegative(unrounded).ShouldBeTrue();

        var rounded = MonetaryRounding.Round(unrounded);

        rounded.ShouldBe(0.0d);

        // A payload carrying -0 is formatted as -$0.00, so the rounded zero must not carry the sign.
        double.IsNegative(rounded).ShouldBeFalse();
    }

    [Fact]
    public void Should_Pass_A_Non_Finite_Value_Through_Unrounded()
    {
        MonetaryRounding.Round(double.PositiveInfinity).ShouldBe(double.PositiveInfinity);
        double.IsNaN(MonetaryRounding.Round(double.NaN)).ShouldBeTrue();
    }
}
