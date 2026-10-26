using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Concerns.Middleware;

internal sealed class CorrelationIdMiddleware : IMiddleware
{
    internal const string HeaderName = "X-Correlation-Id";

    internal const int MaxCorrelationIdLength = 128;

    // The characters IsValidCorrelationId accepts, as a pattern for the OpenAPI document. Not used to validate;
    // CorrelationIdMiddlewareFixture asserts the two agree on every printable ASCII character.
    internal const string AllowedCharactersPattern = "^[A-Za-z0-9_.:-]+$";

    private readonly IProblemDetailsService _problemDetailsService;

    public CorrelationIdMiddleware(IProblemDetailsService problemDetailsService)
    {
        problemDetailsService.WhenNotNull();

        _problemDetailsService = problemDetailsService;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        // Echoed so a caller that sent no id still has one to quote from a response with no error body, and so a
        // caller whose id was rejected learns the generated one. Written as the response starts rather than now,
        // because the exception handler clears the response headers before writing its error - registering it here
        // means the 500 still carries it. Registered before the id is checked, so the rejection below carries it too.
        httpContext.Response.OnStarting(static state =>
        {
            var context = (HttpContext)state;

            context.Response.Headers[HeaderName] = context.TraceIdentifier;

            return Task.CompletedTask;
        }, httpContext);

        if (httpContext.Request.TryGetCorrelationId(out var correlationId))
        {
            if (correlationId.Length > MaxCorrelationIdLength)
            {
                await WriteBadRequestAsync(httpContext, $"CorrelationId exceeds max length of {MaxCorrelationIdLength} chars");

                return;
            }

            // The value is client supplied and is written into every log entry for the request.
            if (!IsValidCorrelationId(correlationId))
            {
                await WriteBadRequestAsync(httpContext, "CorrelationId may only contain letters, digits, '-', '_', '.' and ':'");

                return;
            }

            httpContext.TraceIdentifier = correlationId;
        }

        // Tagged so a trace can be found by the id a user quotes. The hosting layer only creates the request's
        // Activity when something would observe it - a listener, or its own diagnostics logger being enabled - so
        // there may be none, and a request must not fail for that. Log entries do not depend on this tag:
        // CorrelationIdLogEnricher reads TraceIdentifier directly.
        var activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;

        activity?.AddTag("correlationId", httpContext.TraceIdentifier);

        await next(httpContext);
    }

    private static bool IsValidCorrelationId(string correlationId)
    {
        foreach (var character in correlationId)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.' or ':'))
            {
                return false;
            }
        }

        return true;
    }

    private async Task WriteBadRequestAsync(HttpContext httpContext, string detail)
    {
        // Keyed by the header the caller sent, so the reason is found where every other field error is. The rejected
        // value is deliberately not echoed: it is whatever the caller sent, and refusing it is the point.
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Extensions = new Dictionary<string, object?>
            {
                { ValidationErrors.Extension, new Dictionary<string, string[]> { [HeaderName] = [detail] } }
            }
        };

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        await _problemDetailsService.WriteAsync(problemDetailsContext);
    }
}
