using FluentAssertions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Errors;

public class ApiBasicErrorFixture : PotFixtureBase
{
    public class Constructor : ApiBasicErrorFixture
    {
        [Fact]
        public void Should_Set_ErrorType_To_Match_ErrorType()
        {
            var errorType = Create<ErrorType>();

            var error = new ApiBasicError(errorType)
            {
                ErrorCode = "TestCode",
                ErrorMessage = "Test message"
            };

            error.ErrorType.Should().Be(errorType);
        }
    }
}
