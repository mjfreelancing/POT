using Pot.App.Errors;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Errors;

public class ApiDetailErrorFixture : PotFixtureBase
{
    public class Constructor : ApiDetailErrorFixture
    {
        [Fact]
        public void Should_Set_ErrorType_To_Match_ErrorType()
        {
            var errorType = Create<ErrorType>();

            var error = new ApiDetailError(errorType)
            {
                ErrorCode = "TestCode",
                ErrorMessage = "Test message"
            };

            error.ErrorType.ShouldBe(errorType);
        }
    }
}
