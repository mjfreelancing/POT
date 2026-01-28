using FluentAssertions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Errors;

public class ProblemDetailsErrorBaseFixture : PotFixtureBase
{
    public class ClassAttributes : ProblemDetailsErrorBaseFixture
    {
        [Fact]
        public void Should_Be_Abstract()
        {
            typeof(ProblemDetailsErrorBase).IsAbstract.Should().BeTrue();
        }
    }

    public class Constructor : ProblemDetailsErrorBaseFixture
    {
        private class ConcreteProblemDetailsError : ProblemDetailsErrorBase
        {
            public ConcreteProblemDetailsError(ProblemType problemType)
                : base(problemType)
            {
            }
        }

        [Fact]
        public void Should_Set_ErrorType_To_Match_ProblemType()
        {
            var problemType = Create<ProblemType>();

            var error = new ConcreteProblemDetailsError(problemType);

            error.ErrorType.Should().Be(problemType);
        }
    }
}
