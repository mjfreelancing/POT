---
id: ADR-0054
title: Server telemetry is instrumented with .NET's built-in APIs and exported through OpenTelemetry
status: proposed
date: 2026-11-06
area: backend
related-prds: [PRD-0035]
---

# ADR-0054: Server telemetry is instrumented with .NET's built-in APIs and exported through OpenTelemetry

**Summary:** In the context of an API whose only diagnostic signal is console logs joined by a
correlation identifier that is lost once work leaves the request, facing failures and latency that a log
line cannot place, we decided to add traces and metrics, instrumented with .NET's `ActivitySource`,
`Meter` and `ILogger` and exported by the OpenTelemetry SDK over OTLP, to achieve a view of each
request's path, timing and outcome, including the work it queues, without binding application code to a
monitoring vendor, accepting a pipeline to configure, ingestion cost to bound by sampling, and a second
place where sensitive values could land.

## Context and Problem Statement

The server writes console logs only: single-line text in Development, JSON elsewhere, read from Container
Apps into Log Analytics (`WebApplicationBuilderExtensions.AddLogging`). Entries are joined by a
correlation identifier that `CorrelationIdLogEnricher` reads from the current `HttpContext`.

- **Queued work loses the join.** Email is queued in process and sent by a channel reader
  ([ADR-0027](ADR-0027-email-is-queued-in-process.md)), and workers run inside the API
  ([ADR-0026](ADR-0026-background-work-runs-inside-the-api-process.md)). Neither has an `HttpContext`, so
  their entries carry no correlation identifier, and a failed email cannot be traced to the request that
  queued it.
- **Logs show that something happened, not where the time went.** A slow request cannot be broken down
  into database, SMTP and application time, and error and latency rates exist only as log queries.
- **Call logging stands in for tracing.** 175 `LogCall` sites at `Information` record which method ran,
  which is what a span records, with timing and parentage as well.

## Decision Drivers

- A user's report must be traceable through the request and any work it queued.
- Application code must not depend on a monitoring vendor; the backend may change.
- Development and tests must work with no telemetry backend running.
- Ingestion cost must be bounded and predictable.

## Considered Options

1. Console logs with the correlation identifier (status quo).
2. .NET's built-in APIs, with the OpenTelemetry SDK in the host exporting over OTLP.
3. The Azure Monitor OpenTelemetry distribution, exporting from the process to Application Insights.
4. The classic Application Insights SDK.
5. Logs only, with the correlation identifier carried into queued work.

## Decision Outcome

Chosen option: 2, because the built-in APIs are what the framework and its libraries already emit, and
OTLP is accepted by every mainstream backend, so the backend stays a deployment choice.

- **Code instruments with `ActivitySource`, `Meter` and `ILogger` only.** Only the host project
  references OpenTelemetry packages; the application, data and email projects reference none
  ([ADR-0003](ADR-0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md)).
- **The host registers every source and meter the components declare.** A source the host does not
  register emits nothing.
- **The host enables automatic instrumentation** for inbound ASP.NET Core requests, outgoing HTTP and
  Npgsql.
- **W3C trace context links the signals.** A log entry written inside an activity carries its trace and
  span identifiers.
- **Queued work keeps its parent.** Whatever queues work captures the current activity context with it,
  and the consumer starts its own activity linked to that context.
- **Export is OTLP, configured by the standard `OTEL_*` settings.** With no endpoint configured nothing
  is exported, and console logging stays the baseline in every environment, so an exporter outage loses
  traces and metrics, not logs.
- **Sampling is parent-based, with a configured ratio.**
- **Telemetry carries no value that a log may not.** The data rule in
  [PRD-0035](../prd/PRD-0035-telemetry-tracing-and-redaction.md) applies to span tags, span events and
  metric dimensions as well as to logs. Baggage carries no application data.

The backend, the sampling ratio, whether logs are exported over OTLP as well as to the console, and
whether the trace identifier replaces the correlation identifier are decided in PRD-0035.

### Consequences

- Good, because a request's path and timing are visible, database and SMTP calls included.
- Good, because a failed email or worker run joins the trace of the request that caused it.
- Good, because application code depends on no vendor, and the backend changes by configuration.
- Good, because request rate, latency and error rate come from metrics rather than log queries.
- Bad, because the host gains packages and configuration, and the exporter needs an endpoint in each
  deployed environment.
- Bad, because head sampling decides before the outcome is known, so a failed request can go unsampled
  unless a collector samples by outcome.
- Bad, because traces are a second store where sensitive values could land, and automatic
  instrumentation adds attributes nobody wrote, such as URL query strings and SQL text, which must be
  configured or filtered.
- Bad, because two identifiers, the correlation identifier and the trace identifier, coexist until
  PRD-0035 settles them.

### Confirmation

- Test: an integration fixture with an `ActivityListener` asserts that a request produces a server span
  with a database child span, that a request which queues an email produces a consumer span linked to
  the request's trace.
- Test: a fixture asserts that every `ActivitySource` and `Meter` name declared in the server assemblies
  is registered by the host.
- Test: a fixture asserts that no server project other than the host references an OpenTelemetry
  package.
- Instruction: `.github/instructions/server.instructions.md` names the source to use and the rule for
  queued work, citing this record once it is accepted.
- Review: a source or meter the host does not register, or work queued without its activity context, is
  a defect.

## Pros and Cons of the Options

### Console logs with the correlation identifier (status quo)

- Good: no new dependency, cost or configuration.
- Bad: no timing breakdown, no metrics, and queued work cannot be joined to its request.

### Azure Monitor OpenTelemetry distribution

- Good: one call configures traces, metrics and logs for Application Insights.
- Bad: the exporter is fixed in the process, so a backend change is a code change, and local runs need an
  Azure resource or a disabled exporter.

### Classic Application Insights SDK

- Good: familiar, and integrated with the portal.
- Bad: a vendor API in application code, and Microsoft directs new applications to OpenTelemetry instead.

### Logs only, with the correlation identifier carried into queued work

- Good: closes the queued-work gap with the least change.
- Bad: still no timing breakdown or metrics, and it rebuilds by hand what trace context provides.

## More Information

- The request log stays as ADR-0050 decides.
- How sources and meters are named, and whether the health probes produce spans, are settled in
  PRD-0035. One source and one meter per assembly, named after it (`Pot.App`, `Pot.EmailSender`,
  `Pot.AspNetCore`), is the starting proposal.
- Whether a value may enter telemetry, and in what form, is the data rule PRD-0035 settles. If discovery
  finds that rule architectural, it raises its own record.
