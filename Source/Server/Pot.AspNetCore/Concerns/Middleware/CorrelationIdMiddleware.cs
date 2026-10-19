using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Concerns.Middleware;

internal sealed class CorrelationIdMiddleware : IMiddleware
{
    internal const int MaxCorrelationIdLength = 128;

    private readonly IProblemDetailsService _problemDetailsService;

    public CorrelationIdMiddleware(IProblemDetailsService problemDetailsService)
    {
        problemDetailsService.WhenNotNull();

        _problemDetailsService = problemDetailsService;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
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

        var activityFeature = httpContext.Features.GetRequiredFeature<IHttpActivityFeature>();
        var activity = activityFeature.Activity;

        activity.AddTag("correlationId", httpContext.TraceIdentifier);

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
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = detail,
            Status = httpContext.Response.StatusCode
        };

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        await _problemDetailsService.WriteAsync(problemDetailsContext);
    }
}
