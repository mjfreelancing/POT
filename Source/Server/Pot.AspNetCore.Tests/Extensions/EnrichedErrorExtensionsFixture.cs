using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Extensions;
using Shouldly;

namespace Pot.AspNetCore.Tests.Extensions;

public class EnrichedErrorExtensionsFixture
{
    private const string DuplicateValueType = "urn:pot:problem:duplicate-value";
    private const string StaleEntityTagType = "urn:pot:problem:stale-entity-tag";
    private const string DataConflictType = "urn:pot:problem:data-conflict";

    public class ToProblemDetails : EnrichedErrorExtensionsFixture
    {
        [Fact]
        public void Should_Map_An_Entity_Exists_Error_To_A_409_Carrying_The_Message_In_Detail()
        {
            // DEVELOPER.md documents this body. The type names the conflict category and 'detail' is what a
            // client shows, so there is no second place for the message to live.
            var error = ApiDetailErrorFactory.CreateEntityExistsError("Description",
                "The account description already exists");

            var problemDetails = error.ToProblemDetails();

            problemDetails.Status.ShouldBe(409);
            problemDetails.Type.ShouldBe(DuplicateValueType);
            problemDetails.Detail.ShouldBe("The account description already exists");
            problemDetails.Extensions.ShouldNotContainKey("errors");
        }

        [Fact]
        public void Should_Map_A_Stale_Etag_Conflict_To_Its_Own_Type()
        {
            var error = ApiDetailErrorFactory.CreateEtagConflict("Account");

            var problemDetails = error.ToProblemDetails();

            problemDetails.Status.ShouldBe(409);
            problemDetails.Type.ShouldBe(StaleEntityTagType);
            problemDetails.Detail.ShouldBe(error.ErrorMessage);
            problemDetails.Extensions.ShouldNotContainKey("errors");
        }

        [Fact]
        public void Should_Give_Each_Conflict_Category_Its_Own_Type()
        {
            var duplicate = ApiDetailErrorFactory.CreateEntityExistsError("Description", "Exists").ToProblemDetails();
            var stale = ApiDetailErrorFactory.CreateEtagConflict("Account").ToProblemDetails();

            var dataConflictError = new ApiDetailError(ErrorType.Conflict)
            {
                ErrorCode = ErrorCodes.Conflict,
                ErrorMessage = "The request conflicts with the current state of the data."
            };

            var dataConflict = dataConflictError.ToProblemDetails();

            duplicate.Type.ShouldBe(DuplicateValueType);
            stale.Type.ShouldBe(StaleEntityTagType);
            dataConflict.Type.ShouldBe(DataConflictType);

            new[] { duplicate.Type, stale.Type, dataConflict.Type }.Distinct().Count().ShouldBe(3);
        }

        [Fact]
        public void Should_Map_A_Not_Found_Error_Without_Errors()
        {
            var error = ApiDetailErrorFactory.CreateEntityNotFoundError("The account does not exist");

            var problemDetails = error.ToProblemDetails();

            problemDetails.Status.ShouldBe(404);
            problemDetails.Detail.ShouldBe("The account does not exist");
            problemDetails.Extensions.ShouldNotContainKey("errors");
        }

        [Fact]
        public void Should_Keep_The_Field_Errors_On_A_Bad_Request()
        {
            var error = ApiDetailErrorFactory.CreateBadRequestError("Status", "Status cannot be changed");

            var problemDetails = error.ToProblemDetails();

            problemDetails.Status.ShouldBe(400);
            problemDetails.Detail.ShouldBe("Status cannot be changed");

            var errors = problemDetails.Extensions["errors"].ShouldBeOfType<object[]>();
            var details = errors.ShouldHaveSingleItem().ShouldBeOfType<Dictionary<string, object?>>();

            details["propertyName"].ShouldBe("Status");
            details["errorMessage"].ShouldBe("Status cannot be changed");
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
            var error = new ApiDetailError(errorType) { ErrorCode = ErrorCodes.Conflict, ErrorMessage = "Message" };

            error.ToProblemDetails().Status.ShouldBe(expectedStatus);
        }
    }

    public class StatusMapping : EnrichedErrorExtensionsFixture
    {
        [Fact]
        public void Should_Decide_A_Status_For_Every_Error_Type()
        {
            // Adding a category without deciding its status throws here rather than at the first request that
            // produces it.
            foreach (var errorType in Enum.GetValues<ErrorType>())
            {
                var statusCode = ProblemTypes.ToStatusCode(errorType);

                statusCode.ShouldBeOneOf(400, 401, 404, 409, 429, 500);
            }
        }
    }
}
