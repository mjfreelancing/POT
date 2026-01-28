using FluentAssertions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Errors;

public class ProblemDetailsBasicErrorFixture : PotFixtureBase
{
    public class Constructor : ProblemDetailsBasicErrorFixture
    {
        [Fact]
        public void Should_Set_ErrorType_To_Match_ProblemType()
        {
            var problemType = Create<ProblemType>();

            var error = new ProblemDetailsBasicError(problemType)
            {
                ErrorCode = "TestCode",
                ErrorMessage = "Test message"
            };

            error.ErrorType.Should().Be(problemType);
        }
    }
}
