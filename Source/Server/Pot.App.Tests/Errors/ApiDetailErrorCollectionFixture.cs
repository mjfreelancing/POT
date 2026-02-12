using FluentAssertions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Errors;

public class ApiDetailErrorCollectionFixture : PotFixtureBase
{
    public class Constructor : ApiDetailErrorCollectionFixture
    {
        [Fact]
        public void Should_Set_ErrorType_To_Match_ErrorType()
        {
            var errorType = Create<ErrorType>();
            var errors = CreateMany<ApiDetailError>();

            var collection = new ApiDetailErrorCollection(errorType, errors);

            collection.ErrorType.Should().Be(errorType);
        }

        [Fact]
        public void Should_Set_Errors_From_Enumerable()
        {
            var errorType = Create<ErrorType>();
            var errors = CreateMany<ApiDetailError>().ToList();

            var collection = new ApiDetailErrorCollection(errorType, errors);

            collection.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void Should_Create_Empty_Array_When_No_Errors()
        {
            var errorType = Create<ErrorType>();
            var errors = Enumerable.Empty<ApiDetailError>();

            var collection = new ApiDetailErrorCollection(errorType, errors);

            collection.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Should_Create_Array_Copy_Of_Errors()
        {
            var errorType = Create<ErrorType>();
            var errorsList = CreateMany<ApiDetailError>().ToList();

            var collection = new ApiDetailErrorCollection(errorType, errorsList);

            collection.Errors.Should().NotBeSameAs(errorsList);
        }
    }
}
