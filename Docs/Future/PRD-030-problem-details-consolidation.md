# Problem Details Consolidation PRD

**Status:** Planning
**Priority:** High
**Last Updated:** 2026-10-21
**Feature ID:** 030
**Discovery:** complete
**Scope:** Define and enforce one reporting contract for API failures: every failure is an RFC 9457 problem body, no submitted value appears in a body or a log, each status has one documented shape, every conflict names its category in a stable `type`, and the client reads whatever body it receives without rendering markup. It covers the code that builds problem bodies and the shared customisation that completes them, the logging of validation and application failures, the raw request-body logger, the OpenAPI description, the developer guide, and the React client's error parsing. It does not cover the concurrency model (the entity tag stays in the request body and a stale one stays `409`), moving request validation or application-result conversion out of endpoint handlers, the application-layer result library, rate-limit policy values, the mapping from database exceptions to statuses, or whether state-dependent refusals should answer `409` rather than `400`.

## Problem Statement

Error reporting is assembled by hand in several places. The result returns submitted values to the caller, gives one status several shapes, and leaves the client to guess what it received.

**1. Submitted values are returned in error bodies.** Every validation failure carries an `attemptedValue` member holding the value the caller sent. On Change Password, a new password that fails a strength rule is returned in the `400` body, and the cross-field rule that compares the two passwords reports the whole request object, so a failure returns both passwords. The same applies to any other field a validator rejects. A value the server composes into a message it writes can leak the same way: the settings validator interpolates the submitted category into its message.

The codebase already treats the value as sensitive in one place: the validation log deliberately omits it. A second helper, `LoggerExtensions.LogApiError`, writes it. The raw request-body logger goes further and writes every request body, including sign-in and password-reset payloads, whenever the server runs outside Production. A response body is not a safer place than a log: it is held by the browser, visible in developer tools, and recorded by any proxy, APM agent or support tooling that captures traffic.

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

| Concern | Where |
| --- | --- |
| Shared completion (`correlationId`, `instance`) and the writer | `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.AddProblemDetails` |
| Application failure to body | `Pot.AspNetCore/Extensions/EnrichedErrorExtensions.ToProblemDetails` |
| Validation result to body | `Pot.AspNetCore/Concerns/Validation/Extensions/ValidationResultExtensions.ToProblemDetails` |
| Per-handler request validation | `Pot.AspNetCore/Concerns/Validation/ProblemDetailsInspector` |
| Body for the rate limiter, database handler and correlation rejection | `Pot.AspNetCore/Concerns/ProblemDetails/ProblemDetailsContextFactory` |
| Unreadable request | `Pot.AspNetCore/Concerns/ExceptionHandlers/BadRequestExceptionHandler` |
| Error-entry serialisation | `Pot.App/Extensions/ApiDetailsErrorExtensions` |
| Submitted-value logging | `Pot.App/Extensions/LoggerExtensions.LogApiError`; `Pot.AspNetCore/Concerns/Middleware/RawRequestLoggingMiddleware` |
| OpenAPI description | `Pot.AspNetCore/Concerns/OpenApi/ProblemDetailsSchemaTransformer`; `contracts/openapi/v1.json` |
| Client parsing | `Source/Client/pot-react/src/api/errors/apiErrorResponse.ts`; `src/api/interceptors/axiosInterceptors.ts` |

## Terms

- **Problem body** — any error response body, in `application/problem+json`, whatever produced it.
- **Validation problem** — a `400` problem body whose `errors` member maps a field name to the messages for that field, in the shape ASP.NET Core's `HttpValidationProblemDetails` defines.
- **General error key** — the empty string `""`, used as the `errors` key for a message that belongs to the whole request rather than one field.
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

| Status | Default `detail` |
| --- | --- |
| `400` | none — the messages are in `errors` (B1) |
| `401` | Authentication is required. |
| `403` | You do not have permission to perform this action. |
| `404` | The requested resource was not found. |
| `405` | The requested method is not allowed for this resource. |
| `408` | The request timed out. |
| `413` | The request body is too large. |
| `415` | The request content type is not supported. |
| `429` | Too many requests. Please wait and try again. |
| `500` | An unexpected error occurred. |
| any other | The request could not be completed. |

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
- **D4.** `Source/Server/DEVELOPER.md` matches the implementation: no `422`, no `errors` list, no `errorCode` or `attemptedValue`, no guidance to log request bodies, sample bodies in the new shape, and the endpoint example showing the handler pattern this contract uses.
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

## Out of Scope

- **Moving request validation into an endpoint filter, and replacing the per-handler result conversion with one mapping** — a code-quality refactor of its own. Every endpoint whose request type has a validator runs it today; the open risk is a future endpoint forgetting, which belongs with that refactor. Handlers keep their current shape here.
- **Moving the entity tag to an `If-Match` header and answering a stale one `412`** — a concurrency-model change of its own, for a future PRD if wanted. While the tag travels in the body, `409` is the correct status: no precondition header was sent, so none can have failed.
- **Replacing AllOverIt's `EnrichedResult` in the application layer** — it touches every service signature. The current shape is kept.
- **Changing state-dependent refusals from `400` to `409`** — a product decision for a future PRD. "Cannot delete an Account that has linked Expenses" and the administrator lock-out protections stay `400` and are treated as validation problems (B1).
- **Rate-limit policy values and partitioning** — unchanged. Only the shape of the `429` body is in scope (B8, C1).
- **Which database exceptions map to which status** — the database exception handler keeps its mapping rules. Only how it builds the body is in scope (C1, C2).
- **Localised or machine-readable error codes beyond the conflict types** — not needed by the current client. If wanted later, it is an additive change.
- **The health endpoints' failure responses** — a failing readiness check answers `503` with the health check writer's plain-text `Unhealthy`, and stays that way: the callers are infrastructure and the E2E gate, which read the status only. Nothing in B, C or D applies to those two endpoints.
- **The size of the default request body limit** — D6 documents the `413` that Kestrel's 30 MB default produces; whether 30 MB is the right ceiling for JSON endpoints is a separate decision.

## Known Risks and Accepted Limitations

- **A caller that asks for HTML receives JSON.** A browser opening an endpoint URL directly gets a problem body where the framework would have produced a plain-text page. Accepted: the API is JSON-only and the client never requests HTML.
- **A proxy can answer before the application does.** The reverse proxy returns its own HTML page for a `502` or `504` when the API is unreachable, and a failed download answers as a blob, so the client can still receive a body it cannot parse. B10 covers only the responses the application writes; the client reads the status in those cases (E5, E7).
- **A session already open keeps its bundle.** A tab left open across a server release shows generic `400` and `409` wording until it reloads. Accepted: the new bodies stay readable to the old bundle, so the page degrades in wording and never fails.
- **Conflict messages lose the submitted value.** A duplicate reads "The account description already exists" rather than quoting the description. Accepted: quoting would need a per-field judgement about what is safe to repeat, and A1 gives no exception.
- **A state-dependent refusal reads as a validation problem.** Because B1 applies to every `400`, "Cannot delete an Account that has linked Expenses" carries the validation title. Accepted for this contract; making it a `409` is the out-of-scope product decision above.
- **Raw request-body logging is removed.** Diagnosing a deserialisation fault then relies on the status, the correlation id and the exception log. Accepted: a developer can add temporary logging when investigating, and credentials must not be written to a log.

## Assumptions

- The React client in `Source/Client/pot-react` is the only consumer of the API. **The client and server ship together**, so the wire contract changes in one release and the client needs no compatibility path for the previous shape.
- FluentValidation remains the validation library.
- No client code, end-to-end test or external integration reads `errorCode`, `attemptedValue` or custom-state members. Verified for `src/`, the E2E suite and the Postman collection.
- The OpenAPI document is generated from endpoint metadata during a Release build, and the committed copy is the contract of record.
- The writer that answers a request whose `Accept` header names no JSON media type is in place; B10's remaining work is coverage of the `404`, `405` and `text/html` `500` cases.
- Kestrel's default body limit (30 MB) and minimum request-body data rate are what produce `413` and `408` for a body-accepting endpoint; D6 documents those responses, it does not change the limits.

## Decisions Carried Into This Document

| Decision | Resolution |
| --- | --- |
| Shape of `errors` on a `400` | The framework's validation-problem map (`field → messages`), replacing the custom list. It removes the name clash with ASP.NET Core, the custom serialisation and its custom-state camel-casing, and gives the document a known schema. |
| `errors` on other statuses | Removed (B3). Every non-`400` failure has one message, and `detail` carries it. |
| `attemptedValue` | Removed from the wire and from logs (A1, A2). |
| `errorCode` | Removed from the wire (B9). The server keeps its own category constants to choose a status and a problem type; they are not serialised. |
| How a conflict is identified | A `type` per category (B6, B7), the member RFC 9457 defines for identifying a problem type. |
| `type` URI form | `urn:pot:problem:<slug>` — absolute, stable and independent of the deployment host. |
| State-dependent refusals | Stay `400` (Out of Scope); a future PRD decides whether they become `409`. |
| Where the common members are added | One shared customisation, covering every source in C1. |
| Default `detail` per status | The table in B, defined once and shared (C5). |
| Submitted values in messages | Never quoted, including a duplicate description (A1). |
| Raw request-body logging | Removed (A3). |
| Request validation and result conversion | Stay in the endpoint handlers; the filter and single-conversion refactor is out of scope. |
| A request whose `Accept` header excludes JSON | Answered with the problem body anyway (B10). The API is JSON-only, so no caller loses a format it asked for. |
| A body the client cannot read | Treated as absent: the status supplies the message and the body is never rendered (E5, E6). |
| A proxy response with no problem body | Read as the application being unreachable when the status is `502`, `503` or `504` (E7). |
| Client and server release | Together, in one release, so the client carries no compatibility path for the previous wire shape (Assumptions). |

## Manual Test Plan

### Setup

1. Start the full stack locally as described in `Docs/LOCAL-SETUP.md`, and open the client with the browser's developer tools on the Network tab.
2. Sign in as a site administrator, **Admin A**, whose password you know. Keep Scalar (Development) or Postman open with Admin A's bearer token for the requests the client cannot make.
3. Create an account named **Rent**.
4. Create an expense named **Rent Payment**, linked to **Rent**, so a delete-conflict scenario is available.
5. Open **Rent** for editing in two browser tabs, **Tab 1** and **Tab 2**, without saving either.

### A. Submitted values are not echoed

- **A1 — Weak new password.** On Change Password, enter the correct current password and the new password `weakpass1`. Submit.
  Expected: the form shows the strength messages. The `400` response body in the Network tab contains no occurrence of `weakpass1` and no `attemptedValue` member; `errors` is keyed by `newPassword`.
- **A2 — New password equal to current.** On Change Password, enter the same valid password in both fields. Submit.
  Expected: the form shows "The new password must be different from the current password." The response body contains neither password.
- **A3 — A submitted route value in a message.** With Admin A's token, send `PUT /api/settings/not-a-category/anything` with the body `{"value":"1"}`.
  Expected: `400`, and no part of the body contains `not-a-category`.
- **A4 — Server log.** Repeat A1 and A2, then sign in once more, and read the server console output for those requests.
  Expected: the validation entry names the field and message without the submitted value, and no "Inbound request body" entry appears for any of them.

### B. Error shapes

- **B1 — Field validation.** Create an account with an empty description.
  Expected: the form shows the description message. The `400` body has `title` "One or more validation errors occurred.", an `errors` object keyed by `description`, no `detail` member, and `correlationId`, `traceId` and `instance`.
- **B2 — Malformed request.** From Scalar or Postman, send a `POST` to the account-create endpoint with the body `{`.
  Expected: `400`, `application/problem+json`, the validation title, and an `errors` object with a message under the `""` key.
- **B3 — Duplicate name.** Create a second account named **Rent**.
  Expected: `409`. The body has `detail` "The account description already exists", no `errors` member, the framework title "Conflict", and `type` `urn:pot:problem:duplicate-value`. The client shows that message.
- **B4 — Stale entity tag.** Change and save **Rent** in **Tab 1**, then change and save it in **Tab 2**.
  Expected: **Tab 2** shows "A conflicting update has been performed by another user. Refresh and try again." The `409` body carries `type` `urn:pot:problem:stale-entity-tag`.
- **B5 — A refusal about current state.** Delete **Rent**, which still has **Rent Payment** linked.
  Expected: `400`. The message under the `""` key explains that the account has linked expenses; no table name, constraint name or exception text appears.
- **B6 — Not found.** With Admin A's token, request an account by a random GUID.
  Expected: `404` with a `detail`, `correlationId` present, and no `errors` member.
- **B7 — Rate limit.** Send rapid anonymous requests to the sign-in endpoint until one is refused.
  Expected: `429` with a whole-number `Retry-After` header of at least 1. The client shows the rate-limited message, the body's `detail` states the wait, and there is no `errors` member.
- **B8 — A caller that asks for HTML.** From the developer tools console, request `/api/does-not-exist` with `Accept: text/html`.
  Expected: `404`, `application/problem+json`, and a body with a `detail` and `correlationId` — no plain-text page and no empty body.
- **B9 — The proxy answers first.** With the Docker stack running (`Source/Docker`), stop only the API container, reload the client, and open a page that reads data.
  Expected: the page states that the server cannot be reached — no blank error and no raw HTML on screen, and navigation keeps working.

### C. Documentation

- **C1 — OpenAPI.** Open `contracts/openapi/v1.json`.
  Expected: the `400` schema is the validation problem with `errors` required. Both problem schemas list `correlationId`, `traceId` and `instance` as required, non-nullable strings. No operation declares `422`, and no success status references a problem schema. Every operation with a request body declares `413` and `408`, and no `GET` declares either.
- **C2 — Developer guide.** Read the error-handling and endpoint sections of `Source/Server/DEVELOPER.md`.
  Expected: no mention of `422`, `errorCode`, `attemptedValue` or request-body logging. Sample bodies match what B1 and B3 returned, including the conflict `type`. The endpoint example matches the handler pattern in the code.

### Cleanup

- Delete **Rent Payment**, then delete the **Rent** account, and reset **Admin A**'s password if A1 or A2 changed it. Restart the API container stopped in B9.

### Results

| ID | Scenario | Result | Notes |
| --- | --- | --- | --- |
| A1–A4 | Submitted values are not echoed | | |
| B1–B9 | Error shapes | | |
| C1–C2 | Documentation | | |

## Design Notes (Rationale and Rejected Alternatives)

- **Why request validation and result conversion stay in the handlers.** They are duplicated, not wrong: all 24 endpoints whose request type has a validator run it. Moving validation behind a filter would need a per-endpoint context for the two endpoints that derive theirs from route values and the clock, and replacing the per-handler conversion would change the result type of all 47 handlers. Both are maintainability changes independent of this contract, and both are easier once the wire shape is settled.
- **Why the validation-problem map replaced the custom list.** Once `attemptedValue` and `errorCode` are removed, the list carries nothing the map does not, while keeping a custom serialiser, a custom OpenAPI schema and the name clash with the framework. The map is what ASP.NET Core, generated clients and generic readers already understand.
- **Why no submitted value is ever quoted.** A1 has no per-field exception because the server cannot know that a description, a name or an identifier is safe to repeat in every context. "The account description already exists" is the information the user needs; the value is already on their screen.
- **Why `urn:pot:problem:<slug>`.** An absolute URI is the RFC 9457 recommendation, and the URN form makes the identifier permanent without claiming or reserving a DNS name that the deployment would have to keep serving.
