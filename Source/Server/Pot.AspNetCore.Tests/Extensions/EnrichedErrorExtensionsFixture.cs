using Pot.App.Errors;
using Pot.AspNetCore.Extensions;
using Shouldly;

namespace Pot.AspNetCore.Tests.Extensions;

public class EnrichedErrorExtensionsFixture
{
    public class ToProblemDetails : EnrichedErrorExtensionsFixture
    {
        [Fact]
        public void Should_Map_An_Entity_Exists_Error_To_A_409_Carrying_The_Message_Only_In_Errors()
        {
            // DEVELOPER.md documents this body. The factory sets no Description, so there is no 'detail' and a client
            // reads the message from 'errors'.
            var error = ApiDetailErrorFactory.CreateEntityExistsError("Description", "Savings",
                "The account description already exists");

            var problemDetails = error.ToProblemDetails();

            problemDetails.Status.ShouldBe(409);
            problemDetails.Detail.ShouldBeNull();

            var errors = problemDetails.Extensions["errors"].ShouldBeOfType<object[]>();
            var details = errors.ShouldHaveSingleItem().ShouldBeOfType<Dictionary<string, object?>>();

            details["errorCode"].ShouldBe("Conflict");
            details["propertyName"].ShouldBe("Description");
            details["attemptedValue"].ShouldBe("Savings");
            details["errorMessage"].ShouldBe("The account description already exists");
        }

        [Theory]
        [InlineData(ErrorType.BadRequest, 400)]
        [InlineData(ErrorType.Constraint, 400)]
        [InlineData(ErrorType.Auth, 401)]
        [InlineData(ErrorType.NotFound, 404)]
        [InlineData(ErrorType.Conflict, 409)]
        [InlineData(ErrorType.TooManyRequests, 429)]
        [InlineData(ErrorType.Server, 500)]
        public void Should_Map_Each_Error_Type_To_Its_Documented_Status(ErrorType errorType, int expectedStatus)
        {
            // The status table in DEVELOPER.md's Problem Details section. Nothing maps to 422.
            var error = new ApiDetailError(errorType) { ErrorCode = "Code", ErrorMessage = "Message" };

            error.ToProblemDetails().Status.ShouldBe(expectedStatus);
        }
    }
}
