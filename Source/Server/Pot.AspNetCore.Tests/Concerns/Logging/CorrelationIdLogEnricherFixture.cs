using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;
using NSubstitute;
using Pot.AspNetCore.Concerns.Logging;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Logging;

public class CorrelationIdLogEnricherFixture
{
    private readonly HttpContextAccessor _httpContextAccessor = new();
    private readonly IEnrichmentTagCollector _collector = Substitute.For<IEnrichmentTagCollector>();
    private readonly CorrelationIdLogEnricher _enricher;

    public CorrelationIdLogEnricherFixture()
    {
        _enricher = new CorrelationIdLogEnricher(_httpContextAccessor);
    }

    public class Enrich : CorrelationIdLogEnricherFixture
    {
        [Fact]
        public void Should_Add_The_Request_TraceIdentifier()
        {
            // TraceIdentifier is the caller's id once CorrelationIdMiddleware has accepted one, and the value every
            // ProblemDetails body carries - so the log entry and the error body agree. Deliberately no Activity on
            // this context: the hosting layer does not always create one, and logging must not depend on it.
            _httpContextAccessor.HttpContext = new DefaultHttpContext { TraceIdentifier = "support-ticket-4821" };

            _enricher.Enrich(_collector);

            _collector.Received(1).Add("correlationId", "support-ticket-4821");
        }

        [Fact]
        public void Should_Add_Nothing_Outside_A_Request()
        {
            // Background workers log with no request in flight, and have no correlation id to give.
            _enricher.Enrich(_collector);

            _collector.ReceivedCalls().ShouldBeEmpty();
        }
    }
}
