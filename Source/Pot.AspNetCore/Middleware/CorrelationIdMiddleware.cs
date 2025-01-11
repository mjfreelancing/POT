using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

namespace Pot.AspNetCore.Middleware;

public sealed class CorrelationIdMiddleware : IMiddleware
{
    private readonly IProblemDetailsService _problemDetailsService;

    public CorrelationIdMiddleware(IProblemDetailsService problemDetailsService)
    {
        problemDetailsService.WhenNotNull();

        _problemDetailsService = problemDetailsService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var values))
        {
            var correlationId = values.First()!;

            if (correlationId.Length > 128)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Detail = "CorrelationId exceeds max length of 128 chars",
                    Status = context.Response.StatusCode
                };

                var problemDetailsContext = new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                };

                await _problemDetailsService.WriteAsync(problemDetailsContext);

                return;
            }

            context.TraceIdentifier = correlationId;
        }

        var activityFeature = context.Features.GetRequiredFeature<IHttpActivityFeature>();
        var activity = activityFeature.Activity;

        activity.AddTag("correlationId", context.TraceIdentifier);

        await next(context);
    }
}
