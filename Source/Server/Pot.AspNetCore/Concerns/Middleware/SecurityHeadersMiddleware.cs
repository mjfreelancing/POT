using Microsoft.Extensions.Primitives;

namespace Pot.AspNetCore.Concerns.Middleware;

// Adds X-Content-Type-Options: nosniff, so a browser never second-guesses the declared content type and treats a body
// as script or HTML because of what it contains, and Cache-Control: no-store, so responses stay out of browser and
// proxy caches. Almost everything the API returns is a site's financial data or a user's details, login and refresh
// responses carry tokens, and an error body carries the correlation id.
//
// Applied to every response rather than only to /api, because nothing the API serves is meant to be cached and a path
// test is one more thing a new route can fall outside of. An endpoint that genuinely wants caching sets its own
// Cache-Control, and that is left alone. The health probes are the exception: they are short-circuited at routing, so
// they never reach this middleware, and there is nothing user-facing or cacheable in their plain-text status.
internal sealed class SecurityHeadersMiddleware : IMiddleware
{
    internal const string NoSniff = "nosniff";

    internal const string NoStore = "no-store";

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        // Written as the response starts rather than now, because the exception handler clears the response headers
        // before writing a 500 - set now, they would be missing from exactly the responses most worth protecting.
        httpContext.Response.OnStarting(static state =>
        {
            var headers = ((HttpResponse)state).Headers;

            headers.XContentTypeOptions = NoSniff;

            // Only when nothing has chosen a policy already, so an endpoint can opt in to caching. The exception
            // handler sets "no-cache,no-store" on the responses it writes, which already says what this would.
            if (StringValues.IsNullOrEmpty(headers.CacheControl))
            {
                headers.CacheControl = NoStore;
            }

            return Task.CompletedTask;
        }, httpContext.Response);

        await next(httpContext);
    }
}
