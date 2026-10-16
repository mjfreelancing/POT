using Microsoft.AspNetCore.Http;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.ExceptionHandlers;

public class IgnoreExceptionHandlerFixture
{
    public class TryHandleAsync : IgnoreExceptionHandlerFixture
    {
        [Fact]
        public async Task Should_Return_ClientClosedStatus_When_Request_Is_Aborted()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestAborted = new CancellationToken(true)
            };
            var exceptionHandler = new IgnoreExceptionHandler();

            var handled = await exceptionHandler.TryHandleAsync(
                httpContext,
                new OperationCanceledException(),
                CancellationToken.None);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(499);
        }

        [Fact]
        public async Task Should_Handle_TaskCanceledException_When_Request_Is_Aborted()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestAborted = new CancellationToken(true)
            };
            var exceptionHandler = new IgnoreExceptionHandler();

            var handled = await exceptionHandler.TryHandleAsync(
                httpContext,
                new TaskCanceledException(),
                CancellationToken.None);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(499);
        }

        [Fact]
        public async Task Should_Not_Handle_Cancellation_When_Request_Is_Not_Aborted()
        {
            var httpContext = new DefaultHttpContext();
            var exceptionHandler = new IgnoreExceptionHandler();

            var handled = await exceptionHandler.TryHandleAsync(
                httpContext,
                new OperationCanceledException(),
                CancellationToken.None);

            handled.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Not_Handle_NonCancellation_Exception()
        {
            var httpContext = new DefaultHttpContext();
            var exceptionHandler = new IgnoreExceptionHandler();

            var handled = await exceptionHandler.TryHandleAsync(
                httpContext,
                new InvalidOperationException(),
                CancellationToken.None);

            handled.ShouldBeFalse();
        }
    }
}
