using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class IgnoreExceptionHandler : IExceptionHandler
{
    private static readonly Type[] _ignoredExceptionTypes = [typeof(OperationCanceledException), typeof(TaskCanceledException)];

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var ignoreException = _ignoredExceptionTypes.Contains(exception.GetType());

        if (ignoreException)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        return ValueTask.FromResult(ignoreException);
    }
}
