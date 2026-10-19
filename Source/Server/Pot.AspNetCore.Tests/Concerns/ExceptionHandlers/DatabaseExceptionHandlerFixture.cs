using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Npgsql;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.ExceptionHandlers;

public class DatabaseExceptionHandlerFixture
{
    private const string PostgresMessageText = "duplicate key value violates unique constraint \"IX_User_Username\"";

    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
    private readonly FakeLogger<DatabaseExceptionHandler> _logger = new();
    private ProblemDetailsContext? _writtenContext;

    public DatabaseExceptionHandlerFixture()
    {
        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => _writtenContext = context))
            .Returns(ValueTask.FromResult(true));
    }

    public static TheoryData<Exception> ConflictExceptions => new()
    {
        new UniqueConstraintException("Unique violation", CreatePostgresException()),
        new ReferenceConstraintException("Reference violation", CreatePostgresException()),
        new DbUpdateConcurrencyException("Concurrency failure")
    };

    public class TryHandleAsync : DatabaseExceptionHandlerFixture
    {
        [Fact]
        public async Task Should_Log_Error_With_Exception_When_DbUpdateException_Is_Handled()
        {
            var exception = new DbUpdateException("Saving failed");

            var handled = await HandleAsync(exception);

            handled.ShouldBeTrue();

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Level.ShouldBe(LogLevel.Error);
            record.Exception.ShouldBeSameAs(exception);
            record.Message.ShouldContain("POST /api/accounts");
            record.Message.ShouldContain("Saving failed");
        }

        [Fact]
        public async Task Should_Write_Generic_500_When_DbUpdateException_Is_Not_A_Conflict()
        {
            var httpContext = CreateHttpContext();

            await HandleAsync(new DbUpdateException("Saving failed", CreatePostgresException()), httpContext);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

            AssertWrittenProblem(StatusCodes.Status500InternalServerError, ErrorCodes.Database, DatabaseExceptionHandler.DatabaseErrorMessage);
        }

        [Theory]
        [MemberData(nameof(ConflictExceptions))]
        public async Task Should_Write_Generic_409_When_Exception_Is_A_Conflict(Exception exception)
        {
            var httpContext = CreateHttpContext();

            var handled = await HandleAsync(exception, httpContext);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);

            AssertWrittenProblem(StatusCodes.Status409Conflict, ErrorCodes.Conflict, DatabaseExceptionHandler.ConflictMessage);
        }

        [Theory]
        [MemberData(nameof(ConflictExceptions))]
        public async Task Should_Log_Warning_With_Exception_When_Exception_Is_A_Conflict(Exception exception)
        {
            await HandleAsync(exception);

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Level.ShouldBe(LogLevel.Warning);
            record.Exception.ShouldBeSameAs(exception);
        }

        [Fact]
        public async Task Should_Not_Expose_Database_Message_In_Response()
        {
            await HandleAsync(new UniqueConstraintException("Unique violation", CreatePostgresException()));

            var problemDetails = _writtenContext.ShouldNotBeNull().ProblemDetails;
            var errors = problemDetails.Extensions["errors"].ShouldBeOfType<object[]>();
            var error = errors.ShouldHaveSingleItem().ShouldBeOfType<Dictionary<string, object?>>();

            problemDetails.Detail.ShouldNotBeNull().ShouldNotContain("IX_User_Username");
            error["errorMessage"].ShouldBe(DatabaseExceptionHandler.ConflictMessage);
        }

        [Fact]
        public async Task Should_Not_Handle_Or_Log_NonDatabase_Exception()
        {
            var handled = await HandleAsync(new InvalidOperationException());

            handled.ShouldBeFalse();
            _logger.Collector.Count.ShouldBe(0);

            await _problemDetailsService.DidNotReceive().TryWriteAsync(Arg.Any<ProblemDetailsContext>());
        }
    }

    private async Task<bool> HandleAsync(Exception exception, HttpContext? httpContext = null)
    {
        var exceptionHandler = new DatabaseExceptionHandler(_problemDetailsService, _logger);

        return await exceptionHandler.TryHandleAsync(httpContext ?? CreateHttpContext(), exception, CancellationToken.None);
    }

    private void AssertWrittenProblem(int statusCode, string errorCode, string message)
    {
        var problemDetails = _writtenContext.ShouldNotBeNull().ProblemDetails;

        problemDetails.Status.ShouldBe(statusCode);
        problemDetails.Detail.ShouldBe(message);

        var errors = problemDetails.Extensions["errors"].ShouldBeOfType<object[]>();
        var error = errors.ShouldHaveSingleItem().ShouldBeOfType<Dictionary<string, object?>>();

        error["errorCode"].ShouldBe(errorCode);
        error["errorMessage"].ShouldBe(message);
    }

    private static PostgresException CreatePostgresException()
    {
        return new PostgresException(PostgresMessageText, "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/accounts";

        return httpContext;
    }
}
