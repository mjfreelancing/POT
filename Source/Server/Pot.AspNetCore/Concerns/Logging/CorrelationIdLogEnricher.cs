using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace Pot.AspNetCore.Concerns.Logging;

internal class CorrelationIdLogEnricher : ILogEnricher
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
            var httpActivityFeature = httpContext.Features.GetRequiredFeature<IHttpActivityFeature>();
            var activity = httpActivityFeature.Activity;

            var correlationId = activity.GetTagItem("correlationId");

            if (correlationId is not null)
            {
                collector.Add("correlationId", correlationId);
            }
        }
    }
}
