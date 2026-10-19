using AllOverIt.Assertion;
using Microsoft.AspNetCore.Diagnostics;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class BadRequestExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<BadRequestExceptionHandler> _logger;

    public BadRequestExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<BadRequestExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        // The caller sent a request the server could not read. This is not a server fault, so it is
        // logged as a warning, not an error. Logging every malformed request as an error would make
        // real server errors harder to find.
        _logger.LogWarning(exception,
            "Rejected {RequestMethod} {RequestPath} with {StatusCode}: the request could not be read",
            httpContext.Request.Method, httpContext.Request.Path, badRequest.StatusCode);

        httpContext.Response.StatusCode = badRequest.StatusCode;

        // The exception message is written for a developer and can name parameter and type names, so it
        // is not copied into the response.
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = badRequest.StatusCode
        };

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        };

        await _problemDetailsService.WriteAsync(problemDetailsContext);

        return true;
    }
}
