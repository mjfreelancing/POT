using Pot.App.Errors;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Errors;

public class ApiErrorBaseFixture : PotFixtureBase
{
    public class ClassAttributes : ApiErrorBaseFixture
    {
        [Fact]
        public void Should_Be_Abstract()
        {
            typeof(ApiErrorBase).IsAbstract.ShouldBeTrue();
        }
    }

    public class Constructor : ApiErrorBaseFixture
    {
        private class ConcreteApiError : ApiErrorBase
        {
            public ConcreteApiError(ErrorType errorType)
                : base(errorType)
            {
            }
        }

        [Fact]
        public void Should_Set_ErrorType_To_Match_ErrorType()
        {
            var errorType = Create<ErrorType>();

            var error = new ConcreteApiError(errorType);

            error.ErrorType.ShouldBe(errorType);
        }
    }
}
