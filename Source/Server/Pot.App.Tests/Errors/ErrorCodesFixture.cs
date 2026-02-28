using Shouldly;
using Pot.App.Errors;
using Pot.TestUtils;
using System.Reflection;

namespace Pot.App.Tests.Errors;

public class ErrorCodesFixture : PotFixtureBase
{
    public class FieldCount : ErrorCodesFixture
    {
        [Fact]
        public void Should_Have_Expected_Number_Of_Fields()
        {
            var fields = typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            fields.Length.ShouldBe(8, "If this fails, new fields were added. Update the individual field tests below.");
        }
    }

    public class FieldValues : ErrorCodesFixture
    {
        [Fact]
        public void Auth_Should_Match_Field_Name()
        {
            ErrorCodes.Auth.ShouldBe(nameof(ErrorCodes.Auth));
        }

        [Fact]
        public void Invalid_Should_Match_Field_Name()
        {
            ErrorCodes.Invalid.ShouldBe(nameof(ErrorCodes.Invalid));
        }

        [Fact]
        public void NotFound_Should_Match_Field_Name()
        {
            ErrorCodes.NotFound.ShouldBe(nameof(ErrorCodes.NotFound));
        }

        [Fact]
        public void Conflict_Should_Match_Field_Name()
        {
            ErrorCodes.Conflict.ShouldBe(nameof(ErrorCodes.Conflict));
        }

        [Fact]
        public void Constraint_Should_Match_Field_Name()
        {
            ErrorCodes.Constraint.ShouldBe(nameof(ErrorCodes.Constraint));
        }

        [Fact]
        public void Duplicate_Should_Match_Field_Name()
        {
            ErrorCodes.Duplicate.ShouldBe(nameof(ErrorCodes.Duplicate));
        }

        [Fact]
        public void Database_Should_Match_Field_Name()
        {
            ErrorCodes.Database.ShouldBe(nameof(ErrorCodes.Database));
        }

        [Fact]
        public void TooManyRequests_Should_Match_Field_Name()
        {
            ErrorCodes.TooManyRequests.ShouldBe(nameof(ErrorCodes.TooManyRequests));
        }
    }
}
