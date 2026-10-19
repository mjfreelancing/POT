using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.ExceptionHandlers;

public class BadRequestExceptionHandlerFixture
{
    private const string ExceptionMessage = "Failed to read parameter \"Request request\" from the request body.";

    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
    private readonly FakeLogger<BadRequestExceptionHandler> _logger = new();
    private ProblemDetailsContext? _writtenContext;

    public BadRequestExceptionHandlerFixture()
    {
        _problemDetailsService
            .WriteAsync(Arg.Do<ProblemDetailsContext>(context => _writtenContext = context))
            .Returns(ValueTask.CompletedTask);
    }

    public class TryHandleAsync : BadRequestExceptionHandlerFixture
    {
        [Theory]
        [InlineData(StatusCodes.Status400BadRequest)]
        [InlineData(StatusCodes.Status413PayloadTooLarge)]
        public async Task Should_Return_The_Status_The_Exception_Carries(int statusCode)
        {
            var httpContext = CreateHttpContext();

            var handled = await HandleAsync(new BadHttpRequestException(ExceptionMessage, statusCode), httpContext);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(statusCode);
            _writtenContext.ShouldNotBeNull().ProblemDetails.Status.ShouldBe(statusCode);
        }

        [Fact]
        public async Task Should_Not_Expose_The_Exception_Message()
        {
            var httpContext = CreateHttpContext();

            await HandleAsync(new BadHttpRequestException(ExceptionMessage, StatusCodes.Status400BadRequest), httpContext);

            _writtenContext.ShouldNotBeNull().ProblemDetails.Detail.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Log_A_Warning_With_The_Exception()
        {
            var exception = new BadHttpRequestException(ExceptionMessage, StatusCodes.Status413PayloadTooLarge);

            await HandleAsync(exception);

            var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();

            record.Level.ShouldBe(LogLevel.Warning);
            record.Exception.ShouldBeSameAs(exception);
            record.Message.ShouldContain("413");
        }

        [Fact]
        public async Task Should_Not_Handle_Or_Log_Other_Exceptions()
        {
            var handled = await HandleAsync(new InvalidOperationException());

            handled.ShouldBeFalse();
            _logger.Collector.Count.ShouldBe(0);

            await _problemDetailsService.DidNotReceive().WriteAsync(Arg.Any<ProblemDetailsContext>());
        }

        private async Task<bool> HandleAsync(Exception exception, HttpContext? httpContext = null)
        {
            var exceptionHandler = new BadRequestExceptionHandler(_problemDetailsService, _logger);

            return await exceptionHandler.TryHandleAsync(httpContext ?? CreateHttpContext(), exception,
                CancellationToken.None);
        }

        private static DefaultHttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Path = "/api/auth/login";

            return httpContext;
        }
    }
}
