using Pot.Data.Annotations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Annotations;

public class EntityAttributeFixture : PotFixtureBase
{
    public class OtpCode : EntityAttributeFixture
    {
        [Theory]
        [InlineData("123456", true)]
        [InlineData("000000", true)]
        [InlineData("12345", false)]
        [InlineData("1234567", false)]
        [InlineData("12345a", false)]
        // DataAnnotations treats an empty value as valid, so only the pattern is checked here.
        [InlineData("", true)]
        public void Should_Validate_A_Six_Digit_Code(string value, bool expected)
        {
            var attribute = new OtpCodeAttribute();

            var result = attribute.IsValid(value);

            result.ShouldBe(expected);
        }

        [Fact]
        public void Should_Have_A_Descriptive_Error_Message()
        {
            var attribute = new OtpCodeAttribute();

            attribute.ErrorMessage.ShouldBe("OTP code must be exactly 6 digits");
        }
    }

    public class SmallString : EntityAttributeFixture
    {
        [Fact]
        public void Should_Limit_Strings_To_Fifty_Characters()
        {
            var attribute = new SmallStringAttribute();

            attribute.Length.ShouldBe(50);
        }
    }

    public class MediumString : EntityAttributeFixture
    {
        [Fact]
        public void Should_Limit_Strings_To_One_Hundred_Characters()
        {
            var attribute = new MediumStringAttribute();

            attribute.Length.ShouldBe(100);
        }
    }

    public class CiText : EntityAttributeFixture
    {
        [Fact]
        public void Should_Map_The_Column_To_The_Citext_Type()
        {
            var attribute = new CitextAttribute();

            attribute.TypeName.ShouldBe("citext");
        }
    }
}
