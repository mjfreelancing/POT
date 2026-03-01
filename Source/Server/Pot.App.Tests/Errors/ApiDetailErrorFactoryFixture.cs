using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Errors;

public class ApiDetailErrorFactoryFixture : PotFixtureBase
{
    public class CreateEntityExistsError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_Conflict_Type()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityExistsError(propertyName, attemptedValue, errorMessage);

            result.ErrorType.ShouldBe(ErrorType.Conflict);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityExistsError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.Conflict);
            result.PropertyName.ShouldBe(propertyName);
            result.AttemptedValue.ShouldBe(attemptedValue);
            result.ErrorMessage.ShouldBe(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityExistsError(propertyName, null, errorMessage);

            result.AttemptedValue.ShouldBeNull();
        }
    }

    public class CreateEntityNotFoundError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_NotFound_Type()
        {
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityNotFoundError(attemptedValue, errorMessage);

            result.ErrorType.ShouldBe(ErrorType.NotFound);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityNotFoundError(attemptedValue, errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.NotFound);
            result.PropertyName.ShouldBeEmpty();
            result.AttemptedValue.ShouldBe(attemptedValue);
            result.ErrorMessage.ShouldBe(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityNotFoundError(null, errorMessage);

            result.AttemptedValue.ShouldBeNull();
        }
    }

    public class CreateEntityConstraintError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_Constraint_Type()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityConstraintError(propertyName, attemptedValue, errorMessage);

            result.ErrorType.ShouldBe(ErrorType.Constraint);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityConstraintError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.Constraint);
            result.PropertyName.ShouldBe(propertyName);
            result.AttemptedValue.ShouldBe(attemptedValue);
            result.ErrorMessage.ShouldBe(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityConstraintError(propertyName, null, errorMessage);

            result.AttemptedValue.ShouldBeNull();
        }
    }

    public class CreateAuthError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_Auth_Type()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateAuthError(errorMessage);

            result.ErrorType.ShouldBe(ErrorType.Auth);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateAuthError(errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.Auth);
            result.PropertyName.ShouldBeEmpty();
            result.AttemptedValue.ShouldBe(string.Empty);
            result.ErrorMessage.ShouldBe(errorMessage);
        }
    }

    public class CreateUnprocessableEntityError_BasicError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_UnprocessableEntity_Type()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(errorMessage);

            result.ErrorType.ShouldBe(ErrorType.UnprocessableEntity);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.Invalid);
            result.ErrorMessage.ShouldBe(errorMessage);
        }
    }

    public class CreateUnprocessableEntityError_DetailedError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_UnprocessableEntity_Type()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(propertyName, attemptedValue, errorMessage);

            result.ErrorType.ShouldBe(ErrorType.UnprocessableEntity);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.ShouldBe(ErrorCodes.Invalid);
            result.PropertyName.ShouldBe(propertyName);
            result.AttemptedValue.ShouldBe(attemptedValue);
            result.ErrorMessage.ShouldBe(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(propertyName, null, errorMessage);

            result.AttemptedValue.ShouldBeNull();
        }
    }

    public class CreateEtagConflict : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_Conflict_Type()
        {
            var entityType = Create<string>();
            var attemptedValue = Create<int>();

            var result = ApiDetailErrorFactory.CreateEtagConflict(entityType, attemptedValue);

            result.ErrorType.ShouldBe(ErrorType.Conflict);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var entityType = Create<string>();
            var attemptedValue = Create<int>();

            var result = ApiDetailErrorFactory.CreateEtagConflict(entityType, attemptedValue);

            result.ErrorCode.ShouldBe(ErrorCodes.Conflict);
            result.PropertyName.ShouldBe(nameof(EntityBase.Etag));
            result.AttemptedValue.ShouldBe(attemptedValue);
            result.ErrorMessage.ShouldBe($"The entity tag for the {entityType} does not match the current record");
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var entityType = Create<string>();

            var result = ApiDetailErrorFactory.CreateEtagConflict(entityType, null);

            result.AttemptedValue.ShouldBeNull();
        }
    }

    public class CreateTooManyRequests : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_TooManyRequests_Type()
        {
            var totalSeconds = Create<double>();

            var result = ApiDetailErrorFactory.CreateTooManyRequests(totalSeconds);

            result.ErrorType.ShouldBe(ErrorType.TooManyRequests);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var totalSeconds = Create<double>();

            var result = ApiDetailErrorFactory.CreateTooManyRequests(totalSeconds);

            result.ErrorCode.ShouldBe(ErrorCodes.TooManyRequests);
            result.ErrorMessage.ShouldBe($"Too many requests. Please wait and try again after {totalSeconds} seconds.");
        }
    }
}
