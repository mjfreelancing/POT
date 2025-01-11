using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Pot.AspNetCore.ProblemDetails;
using System.Net;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class UnhandledExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILoggerFactory _loggerFactory;

    public UnhandledExceptionHandler(IProblemDetailsService problemDetailsService, ILoggerFactory loggerFactory)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _loggerFactory = loggerFactory.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _loggerFactory
            .CreateLogger<UnhandledExceptionHandler>()
            .LogAllExceptions(exception, null);

        var problemContext = ProblemDetailsContextFactory.Create(httpContext, exception, (int)HttpStatusCode.InternalServerError);

        return await _problemDetailsService.TryWriteAsync(problemContext);
    }
}
