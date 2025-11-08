using Microsoft.AspNetCore.Diagnostics;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class IgnoreExceptionHandler : IExceptionHandler
{
    private static readonly Type[] IgnoredExceptionTypes = [typeof(OperationCanceledException), typeof(TaskCanceledException)];

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var ignoreException = IgnoredExceptionTypes.Contains(exception.GetType());

        if (ignoreException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
        }

        return ValueTask.FromResult(ignoreException);
    }
}
