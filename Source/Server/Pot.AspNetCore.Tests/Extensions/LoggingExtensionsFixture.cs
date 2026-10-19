using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Pot.AspNetCore.Extensions;
using Shouldly;

namespace Pot.AspNetCore.Tests.Extensions;

public class LoggingExtensionsFixture
{
    private const string Secret = "p@ssw0rd!";
    private const string ErrorCode = "NotEmptyValidator";
    private const string PropertyName = "Password";
    private const string ErrorMessage = "'Password' must not be empty.";

    private readonly FakeLogger<LoggingExtensionsFixture> _logger = new();

    public class LogErrors : LoggingExtensionsFixture
    {
        [Fact]
        public void Should_Log_One_Entry_Per_Error_With_Code_Property_And_Message()
        {
            var problemDetails = CreateProblemDetails(
                CreateError(ErrorCode, PropertyName, ErrorMessage),
                CreateError("MinimumLengthValidator", "NewPassword", "'NewPassword' must be at least 8 characters."));

            _logger.LogErrors(problemDetails);

            var records = _logger.Collector.GetSnapshot();

            records.Count.ShouldBe(2);
            records.ShouldAllBe(record => record.Level == LogLevel.Information);
            records[0].Message.ShouldContain(ErrorCode);
            records[0].Message.ShouldContain(PropertyName);
            records[0].Message.ShouldContain(ErrorMessage);
        }

        [Fact]
        public void Should_Not_Log_AttemptedValue()
        {
            var problemDetails = CreateProblemDetails(CreateError(ErrorCode, PropertyName, ErrorMessage));

            _logger.LogErrors(problemDetails);

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Message.ShouldNotContain(Secret);
        }

        [Fact]
        public void Should_Not_Log_When_Errors_Is_Missing()
        {
            var problemDetails = new ProblemDetails();

            _logger.LogErrors(problemDetails);

            _logger.Collector.Count.ShouldBe(0);
        }

        [Fact]
        public void Should_Not_Log_When_Errors_Is_Not_A_Collection_Of_Details()
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Extensions["errors"] = "not an error collection";

            _logger.LogErrors(problemDetails);

            _logger.Collector.Count.ShouldBe(0);
        }

        [Fact]
        public void Should_Ignore_Entries_That_Are_Not_Dictionaries()
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Extensions["errors"] = new object[]
            {
                CreateError(ErrorCode, PropertyName, ErrorMessage),
                42
            };

            _logger.LogErrors(problemDetails);

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Message.ShouldContain(ErrorMessage);
        }

        [Fact]
        public void Should_Log_When_A_Detail_Is_Missing_Keys()
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Extensions["errors"] = new object[] { new Dictionary<string, object?>() };

            _logger.LogErrors(problemDetails);

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Message.ShouldNotBeNullOrWhiteSpace();
        }

        private static ProblemDetails CreateProblemDetails(params Dictionary<string, object?>[] errors)
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Extensions["errors"] = errors;

            return problemDetails;
        }

        private static Dictionary<string, object?> CreateError(string errorCode, string propertyName,
            string errorMessage)
        {
            return new Dictionary<string, object?>
            {
                ["errorCode"] = errorCode,
                ["propertyName"] = propertyName,
                ["attemptedValue"] = Secret,
                ["errorMessage"] = errorMessage
            };
        }
    }
}
