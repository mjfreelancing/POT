using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.ExceptionHandlers;

public class DatabaseExceptionHandlerFixture
{
    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
    private readonly FakeLogger<DatabaseExceptionHandler> _logger = new();

    public DatabaseExceptionHandlerFixture()
    {
        _problemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(ValueTask.FromResult(true));
    }

    public class TryHandleAsync : DatabaseExceptionHandlerFixture
    {
        [Fact]
        public async Task Should_Log_Error_With_Exception_When_DbUpdateException_Is_Handled()
        {
            var httpContext = CreateHttpContext();
            var exception = new DbUpdateException("Saving failed");
            var exceptionHandler = new DatabaseExceptionHandler(_problemDetailsService, _logger);

            var handled = await exceptionHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            handled.ShouldBeTrue();

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Level.ShouldBe(LogLevel.Error);
            record.Exception.ShouldBeSameAs(exception);
            record.Message.ShouldContain("POST /api/accounts");
            record.Message.ShouldContain("Saving failed");
        }

        [Fact]
        public async Task Should_Log_Error_When_Derived_DbUpdateException_Is_Handled()
        {
            var httpContext = CreateHttpContext();
            var exception = new DbUpdateConcurrencyException("Concurrency failure");
            var exceptionHandler = new DatabaseExceptionHandler(_problemDetailsService, _logger);

            var handled = await exceptionHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            handled.ShouldBeTrue();
            _logger.Collector.GetSnapshot().ShouldHaveSingleItem().Exception.ShouldBeSameAs(exception);
        }

        [Fact]
        public async Task Should_Write_ProblemDetails_With_Status_500_When_DbUpdateException_Is_Handled()
        {
            var httpContext = CreateHttpContext();
            var exceptionHandler = new DatabaseExceptionHandler(_problemDetailsService, _logger);

            await exceptionHandler.TryHandleAsync(httpContext, new DbUpdateException("Saving failed"), CancellationToken.None);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

            await _problemDetailsService
                .Received(1)
                .TryWriteAsync(Arg.Is<ProblemDetailsContext>(context => context.ProblemDetails.Status == StatusCodes.Status500InternalServerError));
        }

        [Fact]
        public async Task Should_Not_Handle_Or_Log_NonDatabase_Exception()
        {
            var httpContext = CreateHttpContext();
            var exceptionHandler = new DatabaseExceptionHandler(_problemDetailsService, _logger);

            var handled = await exceptionHandler.TryHandleAsync(httpContext, new InvalidOperationException(), CancellationToken.None);

            handled.ShouldBeFalse();
            _logger.Collector.Count.ShouldBe(0);

            await _problemDetailsService.DidNotReceive().TryWriteAsync(Arg.Any<ProblemDetailsContext>());
        }
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/accounts";

        return httpContext;
    }
}
