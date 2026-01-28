using FluentAssertions;
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

            fields.Should().HaveCount(8, "If this fails, new fields were added. Update the individual field tests below.");
        }
    }

    public class FieldValues : ErrorCodesFixture
    {
        [Fact]
        public void Auth_Should_Match_Field_Name()
        {
            ErrorCodes.Auth.Should().Be(nameof(ErrorCodes.Auth));
        }

        [Fact]
        public void Invalid_Should_Match_Field_Name()
        {
            ErrorCodes.Invalid.Should().Be(nameof(ErrorCodes.Invalid));
        }

        [Fact]
        public void NotFound_Should_Match_Field_Name()
        {
            ErrorCodes.NotFound.Should().Be(nameof(ErrorCodes.NotFound));
        }

        [Fact]
        public void Conflict_Should_Match_Field_Name()
        {
            ErrorCodes.Conflict.Should().Be(nameof(ErrorCodes.Conflict));
        }

        [Fact]
        public void Constraint_Should_Match_Field_Name()
        {
            ErrorCodes.Constraint.Should().Be(nameof(ErrorCodes.Constraint));
        }

        [Fact]
        public void Duplicate_Should_Match_Field_Name()
        {
            ErrorCodes.Duplicate.Should().Be(nameof(ErrorCodes.Duplicate));
        }

        [Fact]
        public void Database_Should_Match_Field_Name()
        {
            ErrorCodes.Database.Should().Be(nameof(ErrorCodes.Database));
        }

        [Fact]
        public void TooManyRequests_Should_Match_Field_Name()
        {
            ErrorCodes.TooManyRequests.Should().Be(nameof(ErrorCodes.TooManyRequests));
        }
    }
}
