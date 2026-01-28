using FluentAssertions;
using FluentValidation.Results;
using Pot.App.Concerns.Validation.Extensions;
using Pot.App.Errors;
using Pot.TestUtils;

namespace Pot.App.Tests.Concerns.Validation.Extensions;

public class ValidationResultExtensionsFixture : PotFixtureBase
{
    public class ToProblemDetailsErrors : ValidationResultExtensionsFixture
    {
        [Fact]
        public void Should_Return_Empty_Collection_When_No_Errors()
        {
            var validationResult = new ValidationResult();

            var result = validationResult.ToProblemDetailsErrors();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Should_Convert_Single_ValidationFailure_To_ProblemDetailsError()
        {
            var validationFailure = new ValidationFailure("Email", "Email is required")
            {
                ErrorCode = "EmailRequired",
                AttemptedValue = ""
            };

            var validationResult = new ValidationResult([validationFailure]);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(1);
            AssertProblemDetailsError(result[0], validationFailure);
        }

        [Fact]
        public void Should_Convert_Multiple_ValidationFailures_To_ProblemDetailsErrors()
        {
            var validationFailures = new[]
            {
                new ValidationFailure("Email", "Email is required")
                {
                    ErrorCode = "EmailRequired",
                    AttemptedValue = ""
                },

                new ValidationFailure("Password", "Password must be at least 8 characters")
                {
                    ErrorCode = "PasswordTooShort",
                    AttemptedValue = "abc"
                },

                new ValidationFailure("Age", "Age must be greater than 0")
                {
                    ErrorCode = "InvalidAge",
                    AttemptedValue = -5
                }
            };

            var validationResult = new ValidationResult(validationFailures);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(3);
            AssertProblemDetailsError(result[0], validationFailures[0]);
            AssertProblemDetailsError(result[1], validationFailures[1]);
            AssertProblemDetailsError(result[2], validationFailures[2]);
        }

        [Fact]
        public void Should_Preserve_CustomState_When_Present()
        {
            var customState = new Dictionary<string, object?>
            {
                { "MinLength", 8 },
                { "MaxLength", 100 }
            };

            var validationFailure = new ValidationFailure("Password", "Password length is invalid")
            {
                ErrorCode = "InvalidPasswordLength",
                AttemptedValue = "abc",
                CustomState = customState
            };

            var validationResult = new ValidationResult([validationFailure]);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(1);
            AssertProblemDetailsError(result[0], validationFailure);
        }

        [Fact]
        public void Should_Handle_Null_AttemptedValue()
        {
            var validationFailure = new ValidationFailure("OptionalField", "Value is invalid")
            {
                ErrorCode = "InvalidValue",
                AttemptedValue = null
            };
            var validationResult = new ValidationResult([validationFailure]);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(1);
            AssertProblemDetailsError(result[0], validationFailure);
        }

        [Fact]
        public void Should_Handle_Various_AttemptedValue_Types()
        {
            var validationFailures = new[]
            {
                new ValidationFailure("StringField", "Invalid string")
                {
                    ErrorCode = "InvalidString",
                    AttemptedValue = "test"
                },

                new ValidationFailure("IntField", "Invalid integer")
                {
                    ErrorCode = "InvalidInt",
                    AttemptedValue = 42
                },

                new ValidationFailure("BoolField", "Invalid boolean")
                {
                    ErrorCode = "InvalidBool",
                    AttemptedValue = true
                },

                new ValidationFailure("DateField", "Invalid date")
                {
                    ErrorCode = "InvalidDate",
                    AttemptedValue = new DateTime(2026, 1, 28)
                }
            };
            var validationResult = new ValidationResult(validationFailures);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(4);
            AssertProblemDetailsError(result[0], validationFailures[0]);
            AssertProblemDetailsError(result[1], validationFailures[1]);
            AssertProblemDetailsError(result[2], validationFailures[2]);
            AssertProblemDetailsError(result[3], validationFailures[3]);
        }

        [Fact]
        public void Should_Set_ErrorType_To_UnprocessableEntity_For_All_Errors()
        {
            var validationFailures = new[]
            {
                new ValidationFailure("Field1", "Error 1") { ErrorCode = "Error1" },
                new ValidationFailure("Field2", "Error 2") { ErrorCode = "Error2" },
                new ValidationFailure("Field3", "Error 3") { ErrorCode = "Error3" }
            };

            var validationResult = new ValidationResult(validationFailures);

            var result = validationResult.ToProblemDetailsErrors().ToList();

            result.Should().HaveCount(3);

            result[0].ErrorType.Should().Be(ProblemType.UnprocessableEntity);
            result[1].ErrorType.Should().Be(ProblemType.UnprocessableEntity);
            result[2].ErrorType.Should().Be(ProblemType.UnprocessableEntity);
        }

        [Fact]
        public void Should_Preserve_All_Properties_From_ValidationFailure()
        {
            var customState = new { AdditionalInfo = "Some info" };

            var validationFailure = new ValidationFailure("TestProperty", "Test error message")
            {
                ErrorCode = "TestErrorCode",
                AttemptedValue = "TestValue",
                CustomState = customState
            };

            var validationResult = new ValidationResult([validationFailure]);

            var result = validationResult.ToProblemDetailsErrors().Single();

            AssertProblemDetailsError(result, validationFailure);
        }

        [Fact]
        public void Should_Return_IEnumerable_That_Can_Be_Enumerated_Multiple_Times()
        {
            var validationFailure = new ValidationFailure("Field", "Error")
            {
                ErrorCode = "ErrorCode"
            };

            var validationResult = new ValidationResult([validationFailure]);

            var result = validationResult.ToProblemDetailsErrors();

            var firstEnumeration = result.ToList();
            var secondEnumeration = result.ToList();

            firstEnumeration.Should().HaveCount(1);
            secondEnumeration.Should().HaveCount(1);
        }

        private static void AssertProblemDetailsError(ProblemDetailsError error, ValidationFailure validationFailure)
        {
            error.ErrorType.Should().Be(ProblemType.UnprocessableEntity);
            error.ErrorCode.Should().Be(validationFailure.ErrorCode);
            error.PropertyName.Should().Be(validationFailure.PropertyName);
            error.AttemptedValue.Should().Be(validationFailure.AttemptedValue);
            error.ErrorMessage.Should().Be(validationFailure.ErrorMessage);
            error.CustomState.Should().BeSameAs(validationFailure.CustomState);
        }
    }
}
