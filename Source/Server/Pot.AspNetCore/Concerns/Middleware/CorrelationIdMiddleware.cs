using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Concerns.Middleware;

internal sealed class CorrelationIdMiddleware : IMiddleware
{
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
            if (correlationId.Length > 128)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Detail = "CorrelationId exceeds max length of 128 chars",
                    Status = httpContext.Response.StatusCode
                };

                var problemDetailsContext = new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                };

                await _problemDetailsService.WriteAsync(problemDetailsContext);

                return;
            }

            httpContext.TraceIdentifier = correlationId;
        }

        var activityFeature = httpContext.Features.GetRequiredFeature<IHttpActivityFeature>();
        var activity = activityFeature.Activity;

        activity.AddTag("correlationId", httpContext.TraceIdentifier);

        await next(httpContext);
    }
}
