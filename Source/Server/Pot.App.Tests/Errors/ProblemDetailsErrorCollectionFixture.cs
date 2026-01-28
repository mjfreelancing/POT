using FluentAssertions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Errors;

public class ProblemDetailsErrorCollectionFixture : PotFixtureBase
{
    public class Constructor : ProblemDetailsErrorCollectionFixture
    {
        [Fact]
        public void Should_Set_ErrorType_To_Match_ProblemType()
        {
            var problemType = Create<ProblemType>();
            var errors = CreateMany<ProblemDetailsError>();

            var collection = new ProblemDetailsErrorCollection(problemType, errors);

            collection.ErrorType.Should().Be(problemType);
        }

        [Fact]
        public void Should_Set_Errors_From_Enumerable()
        {
            var problemType = Create<ProblemType>();
            var errors = CreateMany<ProblemDetailsError>().ToList();

            var collection = new ProblemDetailsErrorCollection(problemType, errors);

            collection.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void Should_Create_Empty_Array_When_No_Errors()
        {
            var problemType = Create<ProblemType>();
            var errors = Enumerable.Empty<ProblemDetailsError>();

            var collection = new ProblemDetailsErrorCollection(problemType, errors);

            collection.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Should_Create_Array_Copy_Of_Errors()
        {
            var problemType = Create<ProblemType>();
            var errorsList = CreateMany<ProblemDetailsError>().ToList();

            var collection = new ProblemDetailsErrorCollection(problemType, errorsList);

            collection.Errors.Should().NotBeSameAs(errorsList);
        }
    }
}
