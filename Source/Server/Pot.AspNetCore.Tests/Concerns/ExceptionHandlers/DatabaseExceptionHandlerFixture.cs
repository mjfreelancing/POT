using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Npgsql;
using NSubstitute;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.ExceptionHandlers;

public class DatabaseExceptionHandlerFixture
{
    private const string PostgresMessageText = "duplicate key value violates unique constraint \"IX_User_Username\"";

    // The conflict a constraint or a race rejected in the database produces, as published to a caller.
    private const string DataConflictType = "urn:pot:problem:data-conflict";

    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
    private readonly FakeLogger<DatabaseExceptionHandler> _logger = new();
    private ProblemDetailsContext? _writtenContext;

    public DatabaseExceptionHandlerFixture()
    {
        // NSubstitute's fluent Returns consumes the substitute's ValueTask once; CA2012 cannot see through the extension.
#pragma warning disable CA2012
        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => _writtenContext = context))
            .Returns(_ => ValueTask.FromResult(true));
#pragma warning restore CA2012
    }

    public enum ConflictKind
    {
        UniqueConstraint,
        ReferenceConstraint,
        Concurrency
    }

    public static TheoryData<ConflictKind> ConflictKinds => [ConflictKind.UniqueConstraint, ConflictKind.ReferenceConstraint, ConflictKind.Concurrency];

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

            AssertWrittenProblem(StatusCodes.Status500InternalServerError, DatabaseExceptionHandler.DatabaseErrorMessage,
                expectedType: null);
        }

        [Theory]
        [MemberData(nameof(ConflictKinds))]
        public async Task Should_Write_Generic_409_When_Exception_Is_A_Conflict(ConflictKind kind)
        {
            var httpContext = CreateHttpContext();

            var handled = await HandleAsync(CreateConflictException(kind), httpContext);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);

            AssertWrittenProblem(StatusCodes.Status409Conflict, DatabaseExceptionHandler.ConflictMessage, DataConflictType);
        }

        [Theory]
        [MemberData(nameof(ConflictKinds))]
        public async Task Should_Log_Warning_With_Exception_When_Exception_Is_A_Conflict(ConflictKind kind)
        {
            var exception = CreateConflictException(kind);

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

            problemDetails.Detail.ShouldNotBeNull().ShouldBe(DatabaseExceptionHandler.ConflictMessage);
            problemDetails.Detail.ShouldNotContain("IX_User_Username");
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

    private void AssertWrittenProblem(int statusCode, string message, string? expectedType)
    {
        var problemDetails = _writtenContext.ShouldNotBeNull().ProblemDetails;

        problemDetails.Status.ShouldBe(statusCode);
        problemDetails.Detail.ShouldBe(message);
        problemDetails.Type.ShouldBe(expectedType);

        // A status other than a 400 states what happened in 'detail' alone.
        problemDetails.Extensions.ShouldNotContainKey("errors");
    }

    private static Exception CreateConflictException(ConflictKind kind)
    {
        return kind switch
        {
            ConflictKind.UniqueConstraint => new UniqueConstraintException("Unique violation", CreatePostgresException()),
            ConflictKind.ReferenceConstraint => new ReferenceConstraintException("Reference violation", CreatePostgresException()),
            ConflictKind.Concurrency => new DbUpdateConcurrencyException("Concurrency failure"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
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
