using Microsoft.AspNetCore.Diagnostics;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class IgnoreExceptionHandler : IExceptionHandler
{
    // 499 is a non-standard status used to record that the client closed the request.
    private const int ClientClosedRequestStatusCode = 499;

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Cancellation can also come from server-side work; only treat it as a client disconnect when the request token was cancelled.
        if (exception is not OperationCanceledException || !httpContext.RequestAborted.IsCancellationRequested)
        {
            return ValueTask.FromResult(false);
        }

        // The status cannot be changed after response headers have been sent.
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.StatusCode = ClientClosedRequestStatusCode;
        }

        return ValueTask.FromResult(true);
    }
}
