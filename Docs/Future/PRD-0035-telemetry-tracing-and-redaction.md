# Telemetry: Tracing and Redaction

Feature ID: 0035
Status: Proposed
Date created: 2026-11-06
Scope: Placeholder for discovery. This document records why the server needs a rule for what may enter
its telemetry, and tracing to go with it; the starting direction agreed so far; and the questions
discovery must settle. Tracing is proposed in
[ADR-0054](../adr/ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md).
Discovery starts once that record is approved, and a full `prd-discovery` cycle produces the requirements.
Audience: Future discovery and implementation agent(s), reviewers, and maintainers.

## Objective

Give the server diagnostics that can trace a user's report to what the server did, without a credential,
user content or clear-text personal data in any telemetry signal: logs, traces or metrics. Deliver the
tracing ADR-0054 proposes, and the data rule that governs every signal.

## Problem To Solve

Credentials are written to the logs today, at `Information`, in every environment:

- the sign-up and password-reset verification handlers and services log the verification code;
- `AuthService.RefreshAsync` logs the raw refresh token, a 30-day credential.

Both arrive through call logging's argument object, which 35 of 175 `LogCall` sites pass. No message
template writes a credential, but about eleven `LogInformation`, `LogWarning` and `LogError` templates
write a username or site name in clear text. Production logs are read from console output into Log
Analytics, where they are kept longer and read more widely than the database.

[ADR-0050](../adr/ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md) keeps bodies and
headers out of the request log, and names these writes as outside its decision. The decision brief DD-08
(answered 2026-11-06) chose option B, stop logging verification codes, and it is carried into this PRD.
The raw refresh token was found later (review finding F-2) and is carried in with it.

Tracing makes the gap wider. Span tags, span events and metric dimensions are exported like log
properties, so a rule that covers logs alone moves the problem rather than solving it.

## Starting Direction

Agreed on 2026-11-06, as a starting point for discovery rather than as requirements:

- **Call logging carries no argument object.** All 35 `LogCall` sites that pass one are changed.
- **Every other log statement is audited in the same change.** Each `LogInformation`, `LogWarning`,
  `LogError` and `LogDebug` call is checked for sensitive values, starting from the eleven known
  templates.
- **Agents get a rule for both.** An instruction file states what call logging and message-template
  logging may carry, so new code follows it without review having to catch it.
- **Values are classified.** A proposed taxonomy, for discovery to confirm:

  | Class | Examples | In telemetry |
  | --- | --- | --- |
  | Operational | row GUIDs, outcomes, statuses, counts, durations, reference codes | As-is |
  | Personal | username, email address, display name, site name | Only redacted |
  | User content | amounts, descriptions, notes | Never |
  | Credential | passwords, temporary passwords, verification codes, tokens, keys, connection strings | Never |

- **Personal data is redacted in process** with `Microsoft.Extensions.Compliance.Redaction`, as a
  classified parameter of a source-generated `[LoggerMessage]` method. A keyed HMAC would keep a
  person's entries searchable without the value being readable.
- **The same rule covers traces and metrics.** The redaction package covers logging only, so span tags
  need a helper that applies the same redactor.
- **Enforcement is mechanical.**
  - A canary integration test drives sign-up, sign-in, refresh and password reset with known values, and
    fails if a credential appears in any captured log or span.
  - A source scan fails when `LogCall` is given an argument object.

## What Is Known

| Area | Fact | Source |
| --- | --- | --- |
| Logging providers | Console only. A single-line formatter in Development; JSON elsewhere, read from Container Apps into Log Analytics | `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs` (`AddLogging`) |
| Request log | Framework HTTP logging: method, path, query, status, duration; no body or headers | ADR-0050 |
| Correlation | `CorrelationIdLogEnricher` tags entries from `HttpContext`, so entries written by queued email or workers carry none | `Pot.AspNetCore/Concerns/Logging/CorrelationIdLogEnricher.cs` |
| Call logging | 175 `LogCall` sites at `Information`; 35 pass an argument object | AllOverIt.Logging 9.4 |
| Credentials in logs | Verification code (four sites), raw refresh token (one site); all through call-logging arguments | DD-08; `AuthService.cs:229` |
| Personal data in logs | Usernames and site names in about eleven message templates, and in call-logging arguments | Repository |
| Source-generated logging | None; no `[LoggerMessage]` methods exist | Repository |
| Tracing | None; no `ActivitySource`, OpenTelemetry or Application Insights package is referenced | Repository |
| Logging rules for agents | Exception logging only | `.github/instructions/allsoverit-patterns.instructions.md` |
| Secrets | Secret values live outside the repository, in a platform store | ADR-0025 |

## Questions Discovery Must Answer

**Data rule**

1. Is the four-class taxonomy right, and which fields belong in each class?
2. Is personal data pseudonymised with a keyed HMAC, or erased? Who holds the key, how is it provisioned
   for development and tests, and is it ever rotated?
3. Does each template that logs a personal value become a classified, source-generated method, or drop
   the value?
4. What is the policy for an exception whose message contains a value?
5. Should call logging stay at `Information`, given 175 sites and the cost of ingestion, or move to
   `Debug`? Does tracing make some of it redundant?
6. Which instruction file carries the agent rule, and does it also become a rule document under
   `Docs/standards/`?
7. Is the data rule architectural enough for its own ADR, beside ADR-0054?
8. How long are logs kept in Log Analytics, and who can read them?

**Tracing (ADR-0054)**

9. Which backend: Application Insights through the Container Apps managed OpenTelemetry agent, a
   collector, or another OTLP endpoint? What does each cost?
10. What sampling ratio? Are failed requests always kept, which needs a collector?
11. Are logs exported over OTLP as well as written to the console?
12. Which spans are wanted beyond the automatic ones? Candidates: email sending, linked to the request
    that queued it; the workers; the sign-up, verification and refresh flows.
13. Does the W3C trace identifier replace the `X-Correlation-Id` the API returns to callers, or sit
    beside it?
14. Is browser-side telemetry in scope?

## Known Risks

- The HMAC redactor may be marked experimental in the package version in use. The fallback is an
  erasing redactor, which loses the ability to join a person's entries.
- Redaction covers source-generated logging only. Plain message templates and span tags depend on the
  agent rule, review and the canary test.
- Exporting traces adds ingestion cost, which sampling has to bound.

## Out Of Scope

- The request log, which stays as ADR-0050 decides.
- Hashing stored verification codes (DD-08 option C), unless discovery chooses to include it. The option:
  HMAC-SHA256 under a server-held key, since an unkeyed hash of a six-digit code is brute-forced
  instantly, with a constant-time comparison; a migration for the column length and a new secret; codes
  active at deploy time stop working. The brief judged it of little value while the temporary password
  is stored only as a hash, so a code alone signs nobody in, and suggested folding it into PRD-0032 if
  that reworks credentials.
- The verification-attempt limit not persisting across requests (review finding F-7).

## Related Documents

- Feature index: [Docs/Future/README.md](README.md)
- Tracing:
  [ADR-0054](../adr/ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md)
- The request log: [ADR-0050](../adr/ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md);
  the contract it came from: [PRD-0030](PRD-0030-problem-details-consolidation.md)
- Codes and tokens: [ADR-0024](../adr/ADR-0024-verification-and-password-reset-use-single-use-expiring-codes.md),
  [ADR-0022](../adr/ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md)
