using Microsoft.AspNetCore.Diagnostics;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class UnhandledExceptionHandler : IExceptionHandler
{
    private static readonly Type[] _handledExceptionTypes = [typeof(BadHttpRequestException)];

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();
        var isHandledException = _handledExceptionTypes.Any(item => exceptionType == item);

        if (!isHandledException)
        {
            return false;
        }

        // Not sending back the message in case it is something like:
        // "Unexpected request without body, failed to bind parameter \"IFormFile file\" from the request body as form."
        // The logging will provide the required detail.

        await Results.Problem(statusCode: 400).ExecuteAsync(httpContext);
        return true;
    }
}
