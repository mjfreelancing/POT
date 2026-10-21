using AllOverIt.Assertion;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace Pot.AspNetCore.Concerns.Logging;

// Adds the request's correlation id to every log entry written while a request is in flight.
//
// Read from TraceIdentifier, which CorrelationIdMiddleware replaces with the caller's id when one is accepted and
// which ProblemDetails bodies already carry - so a log entry and an error body can never disagree. Not read from
// the request's Activity: the hosting layer only creates one when something is observing it, and the middleware
// tags it only after accepting an id, so entries written while rejecting a malformed one would carry nothing.
internal sealed class CorrelationIdLogEnricher : ILogEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdLogEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor.WhenNotNull();
    }

    public void Enrich(IEnrichmentTagCollector collector)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add("correlationId", httpContext.TraceIdentifier);
        }
    }
}
