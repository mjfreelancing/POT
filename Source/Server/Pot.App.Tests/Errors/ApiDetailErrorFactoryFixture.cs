using FluentAssertions;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.TestUtils;

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

            result.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityExistsError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.Conflict);
            result.PropertyName.Should().Be(propertyName);
            result.AttemptedValue.Should().Be(attemptedValue);
            result.ErrorMessage.Should().Be(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityExistsError(propertyName, null, errorMessage);

            result.AttemptedValue.Should().BeNull();
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

            result.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityNotFoundError(attemptedValue, errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.NotFound);
            result.PropertyName.Should().BeEmpty();
            result.AttemptedValue.Should().Be(attemptedValue);
            result.ErrorMessage.Should().Be(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityNotFoundError(null, errorMessage);

            result.AttemptedValue.Should().BeNull();
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

            result.ErrorType.Should().Be(ErrorType.Constraint);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityConstraintError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.Constraint);
            result.PropertyName.Should().Be(propertyName);
            result.AttemptedValue.Should().Be(attemptedValue);
            result.ErrorMessage.Should().Be(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateEntityConstraintError(propertyName, null, errorMessage);

            result.AttemptedValue.Should().BeNull();
        }
    }

    public class CreateAuthError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_Auth_Type()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateAuthError(errorMessage);

            result.ErrorType.Should().Be(ErrorType.Auth);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateAuthError(errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.Auth);
            result.PropertyName.Should().BeEmpty();
            result.AttemptedValue.Should().Be(string.Empty);
            result.ErrorMessage.Should().Be(errorMessage);
        }
    }

    public class CreateUnprocessableEntityError_BasicError : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_UnprocessableEntity_Type()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(errorMessage);

            result.ErrorType.Should().Be(ErrorType.UnprocessableEntity);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.Invalid);
            result.ErrorMessage.Should().Be(errorMessage);
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

            result.ErrorType.Should().Be(ErrorType.UnprocessableEntity);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var propertyName = Create<string>();
            var attemptedValue = Create<int>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(propertyName, attemptedValue, errorMessage);

            result.ErrorCode.Should().Be(ErrorCodes.Invalid);
            result.PropertyName.Should().Be(propertyName);
            result.AttemptedValue.Should().Be(attemptedValue);
            result.ErrorMessage.Should().Be(errorMessage);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var propertyName = Create<string>();
            var errorMessage = Create<string>();

            var result = ApiDetailErrorFactory.CreateUnprocessableEntityError(propertyName, null, errorMessage);

            result.AttemptedValue.Should().BeNull();
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

            result.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var entityType = Create<string>();
            var attemptedValue = Create<int>();

            var result = ApiDetailErrorFactory.CreateEtagConflict(entityType, attemptedValue);

            result.ErrorCode.Should().Be(ErrorCodes.Conflict);
            result.PropertyName.Should().Be(nameof(EntityBase.Etag));
            result.AttemptedValue.Should().Be(attemptedValue);
            result.ErrorMessage.Should().Be($"The entity tag for the {entityType} does not match the current record");
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var entityType = Create<string>();

            var result = ApiDetailErrorFactory.CreateEtagConflict(entityType, null);

            result.AttemptedValue.Should().BeNull();
        }
    }

    public class CreateTooManyRequests : ApiDetailErrorFactoryFixture
    {
        [Fact]
        public void Should_Create_Error_With_TooManyRequests_Type()
        {
            var totalSeconds = Create<double>();

            var result = ApiDetailErrorFactory.CreateTooManyRequests(totalSeconds);

            result.ErrorType.Should().Be(ErrorType.TooManyRequests);
        }

        [Fact]
        public void Should_Set_All_Properties()
        {
            var totalSeconds = Create<double>();

            var result = ApiDetailErrorFactory.CreateTooManyRequests(totalSeconds);

            result.ErrorCode.Should().Be(ErrorCodes.TooManyRequests);
            result.ErrorMessage.Should().Be($"Too many requests. Please wait and try again after {totalSeconds} seconds.");
        }
    }
}
