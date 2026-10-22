# Problem Details Consolidation PRD

**Status:** Planned
**Priority:** High
**Last Updated:** 2026-10-22
**Feature ID:** 030
**Discovery:** complete
**Scope:** Define and enforce one reporting contract for API failures: every failure is an RFC 9457 problem body, no submitted value appears in a body or a log, each status has one documented shape, every conflict names its category in a stable `type`, and the client reads whatever body it receives without rendering markup. It covers the code that builds problem bodies and the shared customisation that completes them, the logging of validation and application failures, HTTP request logging, the OpenAPI description, the developer guide, and the React client's error parsing. It does not cover the concurrency model (the entity tag stays in the request body and a stale one stays `409`), moving request validation or application-result conversion out of endpoint handlers, the application-layer result library, rate-limit policy values, the mapping from database exceptions to statuses, or whether state-dependent refusals should answer `409` rather than `400`.

## Problem Statement

Error reporting is assembled by hand in several places. The result returns submitted values to the caller, gives one status several shapes, and leaves the client to guess what it received.

**1. Submitted values are returned in error bodies.** Every validation failure carries an `attemptedValue` member holding the value the caller sent. On Change Password, a new password that fails a strength rule is returned in the `400` body, and the cross-field rule that compares the two passwords reports the whole request object, so a failure returns both passwords. The same applies to any other field a validator rejects. A value the server composes into a message it writes can leak the same way: the settings validator interpolates the submitted category into its message.

The codebase already treats the value as sensitive in one place: the validation log deliberately omits it. A second helper, `LoggerExtensions.LogApiError`, writes it. The raw request-body logger goes further and writes every request body, including sign-in and password-reset payloads, whenever the server runs outside Production. It is a hand-rolled substitute for the framework's HTTP logging: it buffers the whole body, caps nothing, and — because it sits after authentication and rate limiting — records nothing at all for a request those reject. The API therefore logs its most sensitive content and none of the request metadata a diagnosis usually starts with. A response body is not a safer place than a log: it is held by the browser, visible in developer tools, and recorded by any proxy, APM agent or support tooling that captures traffic.

**2. One status, several shapes.**

- A `400` from request validation carries a `detail` and an `errors` list. A `400` from an unreadable body or a bad-request exception carries no `errors` member at all.
- `errors` is a list of objects, while ASP.NET Core uses the same member name for a field-to-messages map in `HttpValidationProblemDetails`. Anything generic that reads Problem Details — a generated client, a shared error component, the framework's own validation support — expects the map and misreads the list.
- `errorCode` holds either a restatement of the status (`Auth`, `Conflict`, `NotFound`, …) or a FluentValidation validator class name (`NotEmptyValidator`). The client never reads it, and the second kind changes whenever a validator is renamed.
- A stale entity tag is recognised because the conflict's `propertyName` is `Etag` — an undocumented convention that a rename on the server would silently break.
- `409` and `429` bodies also carry `errors`, although neither is a validation failure.

**3. Application failures have nothing to display.** A non-`400` body takes its `detail` from a description no error factory assigns, so a `409` has no `detail` at all. The client compensates by composing "…conflicts with another record that has the same value 'X'" from `propertyName` and `attemptedValue`, so removing the echoed value would leave the client with nothing to show.

**4. Framework-written bodies have nothing to display.** A `401` challenge, `403`, the `404` and `405` the framework writes, a `408`, a `413` and an unhandled `500` carry a status and the framework's title, but no `detail`. A person or a tool reading the body cannot tell what happened.

**5. The contract is undocumented, and the developer guide describes the old shape.** The OpenAPI document does not describe `errors`, and only the maintenance import endpoint declares `413`; no endpoint declares `408`, although any endpoint that reads a body can return both. `Source/Server/DEVELOPER.md` documents the list shape, `errorCode`, `attemptedValue` and the per-handler validation block.

**6. The client assumes every error body is a problem body.** The interceptor casts whatever arrives to the error type; a string satisfies the cast. An HTML page from the reverse proxy, an empty body, or a failed download arriving as a blob is treated as an empty problem, and JSON that is not problem-shaped reads as "the server cannot be reached". Nothing is tested against those shapes.

Where an error body is produced today:

| Concern                                                               | Where                                                                                                                                                                             |
| --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shared completion (`correlationId`, `instance`) and the writer        | `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.AddProblemDetails`                                                                                                     |
| Application failure to body                                           | `Pot.AspNetCore/Extensions/EnrichedErrorExtensions.ToProblemDetails`                                                                                                              |
| Validation result to body                                             | `Pot.AspNetCore/Concerns/Validation/Extensions/ValidationResultExtensions.ToProblemDetails`                                                                                       |
| Per-handler request validation                                        | `Pot.AspNetCore/Concerns/Validation/ProblemDetailsInspector`                                                                                                                      |
| Body for the rate limiter, database handler and correlation rejection | `Pot.AspNetCore/Concerns/ProblemDetails/ProblemDetailsContextFactory`                                                                                                             |
| Unreadable request                                                    | `Pot.AspNetCore/Concerns/ExceptionHandlers/BadRequestExceptionHandler`                                                                                                            |
| Error-entry serialisation                                             | `Pot.App/Extensions/ApiDetailsErrorExtensions`                                                                                                                                    |
| Submitted-value logging and the missing request log                   | `Pot.App/Extensions/LoggerExtensions.LogApiError`; `Pot.AspNetCore/Concerns/Middleware/RawRequestLoggingMiddleware` (hand-rolled; the framework's HTTP logging is not configured) |
| OpenAPI description                                                   | `Pot.AspNetCore/Concerns/OpenApi/ProblemDetailsSchemaTransformer`; `contracts/openapi/v1.json`                                                                                    |
| Client parsing                                                        | `Source/Client/pot-react/src/api/errors/apiErrorResponse.ts`; `src/api/interceptors/axiosInterceptors.ts`                                                                         |

## Terms

- **Problem body** — any error response body, in `application/problem+json`, whatever produced it.
- **Validation problem** — a `400` problem body whose `errors` member maps a field name to the messages for that field, in the shape ASP.NET Core's `HttpValidationProblemDetails` defines.
- **General error key** — the empty string `""`, used as the `errors` key for a message that belongs to the whole request rather than one field. It is the convention ASP.NET Core already uses for model-level validation: MVC's `ModelStateDictionary` files an error added against the model itself (`AddModelError(string.Empty, …)`) under that key, and `ValidationProblemDetails` copies it into the `errors` map unchanged, so the wire body shows `"errors": { "": ["…"] }`. The key is a convention inherited from MVC, not something `HttpValidationProblemDetails` reserves or interprets. POT uses it for a whole-request rule or an application refusal, which FluentValidation does not otherwise place anywhere.
- **Submitted value** — anything the caller sent: a body property, a query or route value, a header.
- **Stale-tag conflict** — a `409` returned because the entity tag in an update request no longer matches the stored record.
- **Fallback conflict** — a `409` the database exception handler writes because a constraint or race reached the database, as opposed to one an endpoint's own rule decided on.
- **Problem type** — the `type` member that identifies a category of failure.

## Functional Requirements

### A. Submitted values never reach a report

- **A1.** No problem body contains a submitted value, whatever the status and whichever part of the server produced it. Field names, messages written by the server, and identifiers the server generated are allowed. No message interpolates a submitted value into its text.
- **A2.** No log entry written for a validation failure or a rejected application result contains a submitted value. Field names, messages and identifiers the server generated are allowed.
- **A3.** No request body is written to a log in any environment.
- **A4.** Automated tests assert A1 and A2 at these points: a weak new password; the new-password-equals-current rule; a setting whose category is not recognised; and the log written for a rejected application result. Each body test asserts that the response does not contain the submitted value and carries no `attemptedValue` member. A test asserts A3 by confirming that no request body is logged for a sign-in or a password change.
- **A5.** Every request other than the health probes produces one request log entry carrying its method, path, query, status code and duration, with the correlation id, and no request or response headers. The health probes produce no request log entry and are outside the application's security-header and correlation middleware; the health check middleware's own no-cache headers remain. Their callers read the status only. A test captures the host's request log and asserts those fields, the absence of a body and headers, and no entry for either probe.
- **A6.** The request log in A5 is produced by the framework's HTTP logging. The hand-rolled request-body logger is removed, and no component reads or buffers a request body in order to log it.

### B. One shape per status

- **B1.** Every `400` is a validation problem: the framework's validation title, and an `errors` member mapping each offending field's name to the messages for that field. A body field is keyed by its camelCase JSON name; a header or route value is keyed by the name the caller sent. A message that belongs to the whole request rather than to a field is filed under the general error key. A `400` carries no `detail` member of its own.
- **B2.** B1 holds for every route into a `400`: request validation, an application result that rejects the request, a body the framework could not read, a rejected `X-Correlation-Id` header, and a bad-request exception.
- **B3.** A problem body for any status other than `400` carries no `errors` member. Its `detail` states what happened in a message written for the user: the application's own message where it has one, otherwise the default for that status. It is never absent and never contains a submitted value, exception text, or a provider, constraint or table name.
- **B4.** `title` is the framework's standard title for the status. Only a `400` deviates, carrying the validation title.
- **B5.** Every problem body carries `correlationId`, `traceId` and `instance`. `instance` is written as the standard Problem Details member, never as an extension, so the body never has two `instance` keys.
- **B6.** A stale-tag conflict is identified by the `type` value `urn:pot:problem:stale-entity-tag`. The client never infers it from a field name.
- **B7.** Every `409` carries a `type` naming its category: `urn:pot:problem:stale-entity-tag`, `urn:pot:problem:duplicate-value`, or `urn:pot:problem:data-conflict` (a constraint or race the database rejected). No two categories share a type.
- **B8.** A `429` carries a `Retry-After` header in whole seconds, rounded up, never less than 1, and a `detail` that states the wait when it is known.
- **B9.** No implementation detail reaches a problem body: no validator class names, exception types, constraint or table names, and no `errorCode` member.
- **B10.** Every error response other than the health endpoints' readiness failure carries `application/problem+json` and a body, whatever the request's `Accept` header contains, including the framework's `404` and `405` and an unhandled `500`. Tests assert a `404` and a `405` for a caller that accepts only `text/html`, and that a `text/html` `500` carries no exception message.

Default `detail` for B3 when the failure supplies no message of its own:

| Status    | Default `detail`                                       |
| --------- | ------------------------------------------------------ |
| `400`     | none — the messages are in `errors` (B1)               |
| `401`     | Authentication is required.                            |
| `403`     | You do not have permission to perform this action.     |
| `404`     | The requested resource was not found.                  |
| `405`     | The requested method is not allowed for this resource. |
| `408`     | The request timed out.                                 |
| `413`     | The request body is too large.                         |
| `415`     | The request content type is not supported.             |
| `429`     | Too many requests. Please wait and try again.          |
| `500`     | An unexpected error occurred.                          |
| any other | The request could not be completed.                    |

### C. The body is built in one place

- **C1.** Every problem body — from every route in B2, the rate limiter, the exception handlers, and the framework's not-found, method-not-allowed, timeout and size-limit responses — gets its common members (B4, B5) from one shared customisation.
- **C2.** No other helper constructs those common members or adds `errors` to a non-`400` body.
- **C3.** The mapping from an application failure category to a status code and problem type exists in exactly one place and covers every category. Adding a category without deciding its status fails a test.
- **C4.** A validation failure is logged once, centrally, at `Information`, with field names and messages only. Handlers do not log it themselves.
- **C5.** The default `detail` for each status in B3 is defined in one place.

### D. The contract is documented

- **D1.** The OpenAPI document describes every `400` as the validation problem schema, with `errors` required, and every problem schema with `correlationId`, `traceId` and `instance` as required, non-nullable strings.
- **D2.** The document describes the stale-tag `type` on the endpoints that can return it, and its description says how to tell a stale-tag `409` from every other conflict.
- **D3.** A metadata test asserts: every API endpoint declares `400` as the validation problem and `500` as a problem in `application/problem+json`; every declared `409` is a problem in `application/problem+json`; no success status is described as a problem; no `422` is declared.
- **D4.** `Source/Server/DEVELOPER.md` matches the implementation: no `422`, no `errors` list, no `errorCode` or `attemptedValue`, no guidance to log request bodies, the request log described as the framework's with the fields it records, sample bodies in the new shape, and the endpoint example showing the handler pattern this contract uses.
- **D5.** Every endpoint that declares a `409` has a test asserting the `type` its own rule produced, so the test fails if the rule stops running. The fallback conflict type cannot satisfy it.
- **D6.** Every endpoint that reads a request body declares `413` and `408` as problems, and no endpoint that reads none declares either. A metadata test asserts both directions, so a new body-accepting endpoint cannot omit them and a `GET` cannot claim them.
- **D7.** `contracts/openapi/v1.json` is regenerated and committed with the change that alters it.

### E. The client reads every response it receives

- **E1.** On a `400` the client shows every message in `errors`, field and general alike.
- **E2.** On a `409` the client shows the stale-tag message ("A conflicting update has been performed by another user. Refresh and try again.") when the body carries the stale-tag type, and the body's `detail` for every other conflict.
- **E3.** The client's error response type describes the new shape: `errors` is a map of field name to messages, and the per-entry type disappears along with the `attemptedValue` and `errorCode` members it carried.
- **E4.** A `401`, `403`, `404`, `405` and `429` keep their current fixed copy. A `5xx` shows the body's `detail` when it carries one, and the current fallback otherwise.
- **E5.** The client reads every error body it can receive without assuming a problem shape: the problem bodies of B1 and B3, JSON that is not problem-shaped, the reverse proxy's HTML or plain-text page, an empty body, and a downloaded file whose error body arrives as a blob. A body it cannot interpret is treated as absent and is never rendered into the page.
- **E6.** One normaliser at the interceptor boundary turns each transport failure into a domain error, with a fixed precedence: the problem body's own members first, the message for the status second, a generic message last. It is total — no failure, whatever its body, leaves a message undefined, empty or filled with markup.
- **E7.** A `502`, `503` or `504` that carries no readable problem body means the application was not reached, and reads as unreachable rather than as a server error with nothing to say.
- **E8.** Unit tests cover each shape in E5 against at least a `400`, a `409`, a `500` and a `504`, asserting the user-visible message rather than the parse internals.

## Non-Functional Requirements

- **N1. Related documentation moves with the change.** Every document that describes the error contract, request logging or the health endpoints is updated in the same delivery: `Source/Server/DEVELOPER.md` (D4), `contracts/openapi/v1.json` (D7), `Docs/ARCHITECTURE.md` where it describes error handling (it names RFC 7807, which RFC 9457 supersedes), and any client or setup documentation that describes error parsing. A repository search for the retired vocabulary — `attemptedValue`, `errorCode`, the `errors` list, "Inbound request body", `422` — finds it only where a historical document records the old behaviour (this PRD's problem statement and superseded notes in other PRDs), never where current behaviour is described.
- **N2. Other PRDs are reviewed, not assumed unaffected.** Before completion, every PRD under `Docs/Future/` that states or depends on an error shape, a status, the request log or the health endpoints is checked, and each is either confirmed unchanged or amended. At least: PRD-014 (its 429 body names an error code), PRD-021 and PRD-023 (historical `422` statements), PRD-022 (deletion refusals and their tests), PRD-026 (the `propertyName == etag` conflict convention and its `422` note) and PRD-027 (refusal statuses and bodies), with PRD-013 and ADR-012 for the probes. Folder conventions apply: a superseded statement is annotated with the later PRD's number rather than rewritten, a `Planned` or `In Progress` PRD whose requirements conflict is updated, and no PRD or ADR reference enters code, tests or comments.
- **N3. Tests come before the code they cover.** A test for behaviour this PRD changes starts red for the expected reason and goes green with the implementation. A test that fills a coverage gap in behaviour that already holds starts green and stays green. The stage report states which case applied, and a test claimed as red is shown failing before the change; a test that passes without exercising the change does not count.

## Out of Scope

- **Moving request validation into an endpoint filter, and replacing the per-handler result conversion with one mapping** — a code-quality refactor of its own. Every endpoint whose request type has a validator runs it today; the open risk is a future endpoint forgetting, which belongs with that refactor. Handlers keep their current shape here.
- **Moving the entity tag to an `If-Match` header and answering a stale one `412`** — a concurrency-model change of its own, for a future PRD if wanted. While the tag travels in the body, `409` is the correct status: no precondition header was sent, so none can have failed.
- **Replacing AllOverIt's `EnrichedResult` in the application layer** — it touches every service signature. The current shape is kept.
- **Changing state-dependent refusals from `400` to `409`** — a product decision for a future PRD. "Cannot delete an Account that has linked Expenses" and the administrator lock-out protections stay `400` and are treated as validation problems (B1).
- **Rate-limit policy values and partitioning** — unchanged. Only the shape of the `429` body is in scope (B8, C1).
- **Which database exceptions map to which status** — the database exception handler keeps its mapping rules. Only how it builds the body is in scope (C1, C2).
- **Localised or machine-readable error codes beyond the conflict types** — not needed by the current client. If wanted later, it is an additive change.
- **The health endpoints** — a failing readiness check answers `503` with the health check writer's plain-text `Unhealthy`, and stays that way: the callers are infrastructure and the E2E gate, which read the status only. Both probes are short-circuited out of the pipeline, so they produce no request log entry and are outside the application's security-header and correlation middleware; the health check middleware's own no-cache headers remain. Nothing in B, C or D applies to them.
- **The size of the default request body limit** — D6 documents the `413` that Kestrel's 30 MB default produces; whether 30 MB is the right ceiling for JSON endpoints is a separate decision.

## Known Risks and Accepted Limitations

- **A caller that asks for HTML receives JSON.** A browser opening an endpoint URL directly gets a problem body where the framework would have produced a plain-text page. Accepted: the API is JSON-only and the client never requests HTML.
- **A proxy can answer before the application does.** The reverse proxy returns its own HTML page for a `502` or `504` when the API is unreachable, and a failed download answers as a blob, so the client can still receive a body it cannot parse. B10 covers only the responses the application writes; the client reads the status in those cases (E5, E7).
- **A session already open keeps its bundle.** A tab left open across a server release shows generic `400` and `409` wording until it reloads. Accepted: the new bodies stay readable to the old bundle, so the page degrades in wording and never fails.
- **Conflict messages lose the submitted value.** A duplicate reads "The account description already exists" rather than quoting the description. Accepted: quoting would need a per-field judgement about what is safe to repeat, and A1 gives no exception.
- **A state-dependent refusal reads as a validation problem.** Because B1 applies to every `400`, "Cannot delete an Account that has linked Expenses" carries the validation title. Accepted for this contract; making it a `409` is the out-of-scope product decision above.
- **Request bodies are never logged.** Diagnosing a deserialisation fault relies on the request log's metadata, the status, the correlation id and the exception log. Accepted: a developer can add temporary body logging locally when investigating, and credentials must not be written to a log. The health probes also stop carrying the application's `X-Content-Type-Options` header and the correlation id; the health check middleware's own no-cache headers remain, and their callers read the status only.

## Assumptions

- The React client in `Source/Client/pot-react` is the only consumer of the API. **The client and server ship together**, so the wire contract changes in one release and the client needs no compatibility path for the previous shape.
- FluentValidation remains the validation library.
- No client code, end-to-end test or external integration reads `errorCode`, `attemptedValue` or custom-state members. Verified for `src/`, the E2E suite and the Postman collection.
- The OpenAPI document is generated from endpoint metadata during a Release build, and the committed copy is the contract of record.
- The writer that answers a request whose `Accept` header names no JSON media type is in place; B10's remaining work is coverage of the `404`, `405` and `text/html` `500` cases.
- Kestrel's default body limit (30 MB) and minimum request-body data rate are what produce `413` and `408` for a body-accepting endpoint; D6 documents those responses, it does not change the limits.

## Decisions Carried Into This Document

| Decision                                      | Resolution                                                                                                                                                                                                                              |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shape of `errors` on a `400`                  | The framework's validation-problem map (`field → messages`), replacing the custom list. It removes the name clash with ASP.NET Core, the custom serialisation and its custom-state camel-casing, and gives the document a known schema. |
| `errors` on other statuses                    | Removed (B3). Every non-`400` failure has one message, and `detail` carries it.                                                                                                                                                         |
| `attemptedValue`                              | Removed from the wire and from logs (A1, A2).                                                                                                                                                                                           |
| `errorCode`                                   | Removed from the wire (B9). The server keeps its own category constants to choose a status and a problem type; they are not serialised.                                                                                                 |
| How a conflict is identified                  | A `type` per category (B6, B7), the member RFC 9457 defines for identifying a problem type.                                                                                                                                             |
| `type` URI form                               | `urn:pot:problem:<slug>` — absolute, stable and independent of the deployment host.                                                                                                                                                     |
| State-dependent refusals                      | Stay `400` (Out of Scope); a future PRD decides whether they become `409`.                                                                                                                                                              |
| Where the common members are added            | One shared customisation, covering every source in C1.                                                                                                                                                                                  |
| Default `detail` per status                   | The table in B, defined once and shared (C5).                                                                                                                                                                                           |
| Submitted values in messages                  | Never quoted, including a duplicate description (A1).                                                                                                                                                                                   |
| Request logging                               | The framework's HTTP logging, with method, path, query, status and duration, one combined entry per request, and no request body or headers in any environment (A3, A5, A6). The health probes are short-circuited out of the pipeline. |
| Request validation and result conversion      | Stay in the endpoint handlers; the filter and single-conversion refactor is out of scope.                                                                                                                                               |
| A request whose `Accept` header excludes JSON | Answered with the problem body anyway (B10). The API is JSON-only, so no caller loses a format it asked for.                                                                                                                            |
| A body the client cannot read                 | Treated as absent: the status supplies the message and the body is never rendered (E5, E6).                                                                                                                                             |
| A proxy response with no problem body         | Read as the application being unreachable when the status is `502`, `503` or `504` (E7).                                                                                                                                                |
| Client and server release                     | Together, in one release, so the client carries no compatibility path for the previous wire shape (Assumptions).                                                                                                                        |

## Manual Test Plan

**Not required.** Every behaviour a manual plan would check is asserted by an automated suite, and those suites are the completion gate:

- Server contract and status shapes: integration tests, including the coverage required by A4, B10, D3, D5 and D6.
- Request logging, the probe exclusion and the absence of a body: an integration test that captures the host's request log (A5, A6).
- OpenAPI: the document and endpoint-metadata fixtures (D1–D3, D6).
- Client parsing and unreadable bodies: the interceptor and error-parser unit tests (E5–E8), including an HTML page, an empty body and a blob.
- Client journeys: the prodlike Playwright suite at completion; the conflict copy is covered by a unit test (E2).
- Developer guide: D4 is a documentation change, reviewed with the change rather than exercised at runtime.

The only environmental difference not reproduced is the reverse proxy's own error page when the API is unreachable. The client unit tests supply the same HTML body and assert the same outcome (E5, E7), so a manual run would repeat the assertion rather than prove anything new.

## Design Notes (Rationale and Rejected Alternatives)

- **Why request validation and result conversion stay in the handlers.** They are duplicated, not wrong: all 24 endpoints whose request type has a validator run it. Moving validation behind a filter would need a per-endpoint context for the two endpoints that derive theirs from route values and the clock, and replacing the per-handler conversion would change the result type of all 47 handlers. Both are maintainability changes independent of this contract, and both are easier once the wire shape is settled.
- **Why the validation-problem map replaced the custom list.** Once `attemptedValue` and `errorCode` are removed, the list carries nothing the map does not, while keeping a custom serialiser, a custom OpenAPI schema and the name clash with the framework. The map is what ASP.NET Core, generated clients and generic readers already understand.
- **Why no submitted value is ever quoted.** A1 has no per-field exception because the server cannot know that a description, a name or an identifier is safe to repeat in every context. "The account description already exists" is the information the user needs; the value is already on their screen.
- **Why `urn:pot:problem:<slug>`.** An absolute URI is the RFC 9457 recommendation, and the URN form makes the identifier permanent without claiming or reserving a DNS name that the deployment would have to keep serving.
- **Why the hand-rolled request-body logger was replaced.** Problem: POT logged raw request bodies with no size cap and no request metadata, so it captured credentials and told a diagnosis nothing about the request itself. Reason: ASP.NET Core's `Microsoft.AspNetCore.HttpLogging` already provides request logging with fields that are chosen rather than inherited, one combined entry per request, and an optional bounded body; the API needs the metadata, not the body. Solution: delete `RawRequestLoggingMiddleware`; register `AddHttpLogging` with method, path, query, status and duration; place `UseHttpLogging` after the security headers and before the exception handler, so a failure is logged with its final status; add the `Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware` category at `Information` in `appsettings.json`, without which the entry is swallowed by `Microsoft.AspNetCore: Warning`; log no body and no headers. The existing correlation-id enricher tags the entry, so header logging is unnecessary.
- **Why the health probes are short-circuited.** They are anonymous, infrastructure-only and polled every few seconds. Logging them is noise, and a security header on a plain-text probe response protects nothing: there is no session, no user content and no browser. `ShortCircuit()` ends the request at routing, so a probe reaches no middleware at all — no request log entry, no `X-Content-Type-Options`, no correlation id (the health check middleware still writes its own no-cache headers) — and its caller reads the status only.
