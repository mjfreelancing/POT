# Problem Details Consolidation PRD

**Status:** Planning
**Priority:** High
**Last Updated:** 2026-10-21
**Feature ID:** 030
**Discovery:** not run — the requirements below are recommendations from a code review of the server's error handling. Every item in **Decisions Carried Into This Document** is a proposal, and the PRD must not move to `Planned` until those proposals and the **Open Questions** have been confirmed or changed.
**Scope:** Change how the server builds and shapes its RFC 9457 Problem Details error responses so that submitted values are never echoed, every error of a given status has one documented shape, an error is written as Problem Details whatever the caller's headers ask for, and failure handling is written once instead of in every endpoint handler. Covers the server's error contract, the code that produces it, the OpenAPI description of it, the developer documentation that describes it, and the React client code that parses it — including how the client reads a response body the application did not write. It does **not** cover the concurrency model (the entity tag stays in the request body and a stale one stays a `409`), the application layer's result library, rate-limit policy values, or the database exception mapping rules — see **Out of Scope**.

## Problem Statement

Error responses are assembled in many places, by hand, and the result is one security defect, a contract that differs by route, and a block of failure-handling code repeated in nearly every endpoint.

**1. Submitted values are echoed back in error responses.** Every error entry carries an `attemptedValue` member holding the value the caller sent. For FluentValidation failures it is filled automatically with the property value:

- `Me/ChangePassword` validates `NewPassword` with `StrongPasswordValidator`. A new password that fails a strength rule is returned in the `400` body as `attemptedValue`.
- The same validator has a cross-field rule on the whole request (`RuleFor(request => request).Must(...)`). For such a rule FluentValidation sets the attempted value to the **request object**, which is then serialised, so a failure would return both `CurrentPassword` and `NewPassword` in the response body.

Both were found by inspection and need a test to confirm (requirement **A3**). The codebase already treats the value as sensitive in one place: `Pot.AspNetCore/Extensions/LoggingExtensions.cs` deliberately does not log it, "for login, signup and password reset it can be a password or an OTP". A second helper, `Pot.App/Extensions/LoggerExtensions.LogApiError`, does log it. A response body is not a safer place than a log: it is held by the browser, visible in developer tools, and recorded by any proxy, APM agent or support tooling that captures traffic.

**2. The wire contract differs by route.**

- A FluentValidation `400` carries `detail: "One or more validation errors occurred."` and an `errors` array. A `400` from a malformed body, from a rejected `X-Correlation-Id`, or from `BadHttpRequestException` carries **no** `errors` member. The client copes by falling back to `detail`/`title`, but the contract has two shapes for one status.
- `errors` is a list of `{errorCode, propertyName, attemptedValue, errorMessage, …custom state}` objects. ASP.NET Core uses the same member name, `errors`, for a different shape (`HttpValidationProblemDetails`, a field-to-messages map). Anything generic that reads Problem Details — a generated client, a shared error component, a future use of the framework's own validation — expects the framework shape and misreads POT's.
- `errorCode` holds either a restatement of the status (`Auth`, `Invalid`, `NotFound`, `Conflict`, `Constraint`, `TooManyRequests`, `Database`) or a FluentValidation validator class name (`NotEmptyValidator`). The client never reads it (`src/api/errors/apiErrorResponse.ts`), and the second kind is an implementation detail that changes whenever a validator is renamed.
- A stale entity tag is recognised by the client because the conflict entry's `propertyName` is `etag` — an undocumented convention that a rename on the server would silently break.

**3. Failure handling is repeated in every handler.** Of 47 handlers, 24 run validation by hand through `IProblemDetailsInspector` and 38 convert a failed application result with `TypedResults.Problem(result.Error!.ToProblemDetails())`. The validation block is:

```csharp
var problemDetails = problemDetailsInspector.Validate(request);

if (problemDetails.IsProblem())
{
    logger.LogErrors(problemDetails);

    return TypedResults.Problem(problemDetails);
}
```

It relies on a sentinel `NoProblemDetails.Single` instance to mean "valid", and the conversion relies on `!` because the result type is not nullability-annotated. A new endpoint that forgets the block is not validated at all, and nothing reports it.

**4. The body is built by more than one helper.** `EnrichedErrorExtensions.ToProblemDetails`, `ValidationResultExtensions.ToProblemDetails`, `ProblemDetailsContextFactory.Create` (used by the rate limiter and `DatabaseExceptionHandler`), `CorrelationIdMiddleware.WriteBadRequestAsync` and `BadRequestExceptionHandler` each construct a body their own way. Only `correlationId` and `instance` are common to all of them, because only those are added by the shared `CustomizeProblemDetails`.

**5. The contract is undocumented, and the documentation is wrong.**

- The committed `contracts/openapi/v1.json` describes `ProblemDetails` with its five standard members only. `errors`, `correlationId` and `traceId` do not appear.
- `Source/Server/DEVELOPER.md` says validation answers **`422`** "never 400", and that the administrator lock-out protections answer `422`. The server answers `400`, and `EndpointResponseMetadataFixture` asserts that no endpoint even declares a `422`. The endpoint example in the same document declares `.ProducesProblem(StatusCodes.Status201Created)`, which describes a successful creation as an error body — the mistake `EndpointResponseMetadataFixture.Should_Not_Describe_Success_Responses_As_ProblemDetails` exists to catch.
- Only the Maintenance import endpoint declares `413`. Every other endpoint that reads a body can return `413` (Kestrel's default 30 MB limit) and `408` (a body arriving below Kestrel's minimum data rate), and the document does not say so. For a bound body the framework sets the status itself and `UseStatusCodePages` writes the body, in every environment (verified against .NET 10 for `413`); `BadRequestExceptionHandler` sees one only when code reads `Request.Body` directly.

**6. An error can arrive with no body, or as plain text.** The framework's problem writer declines a request whose `Accept` header names no JSON media type, so what a caller receives depends on how it asked: `IProblemDetailsService.WriteAsync` throws from inside an exception handler and leaves an empty `500`, `UseStatusCodePages` writes a plain-text page, and `TypedResults.Problem` serialises the body without the shared customisation, so it carries no `correlationId`. A tool or browser that asks for HTML therefore gets either nothing to diagnose with or something that is not the documented contract.

## Terms

- **Problem body** — any error response body, in `application/problem+json`, whatever produced it.
- **Validation problem** — a `400` problem body whose `errors` member maps a field name to the messages for that field, in the shape ASP.NET Core's `HttpValidationProblemDetails` defines.
- **General error key** — the empty string `""`, used as the `errors` key for a message that belongs to the whole request rather than one field. It is the key ASP.NET Core itself uses for model-level errors.
- **Submitted value** — anything the caller sent: a body property, a query or route value, a header.
- **Stale-tag conflict** — a `409` returned because the entity tag in an update request no longer matches the stored record.
- **Fallback conflict** — a `409` the database exception handler writes because a constraint failed, as opposed to one an endpoint's own rule decided on.
- **Unconditional writing** — producing a problem body for every error, including a request whose `Accept` header names no media type the server would otherwise answer with.

## Functional Requirements

### A. Submitted values are never echoed

- **A1.** No problem body contains a submitted value, whatever the status and whichever part of the server produced it. Field names and messages written by the server are allowed, and a rule's message is part of that body, so none interpolates the value through `{PropertyValue}` or `{AttemptedValue}`. A value the server deliberately composes into a message it controls — such as a duplicate description — is the only exception, and only where Open Question 4 says so.
- **A2.** No log entry written for a validation failure or a rejected application result contains a submitted value. Field names, messages and identifiers the server generated are allowed.
- **A3.** Automated tests assert A1 at the two known exposure points: a weak new password on Change Password, and a failure of its cross-field rule. Each asserts that the response body does not contain the password that was sent. The tests are written first and must fail against the current code.

### B. One shape per status

- **B1.** Every `400` is a validation problem: the standard validation title, and an `errors` member keyed by the JSON name of the offending field as the client sent it (camelCase). Messages that name no field are filed under the general error key.
- **B2.** B1 holds for every route into a `400`: request validation, an application result that rejects the request, a body the framework could not read, a rejected `X-Correlation-Id` header, and `BadHttpRequestException`.
- **B3.** A problem body for any status other than `400` carries no `errors` member. Its `detail` is a message written for the user, carries the message of the error that was reported, and is safe to display. It is never absent: a failure with nothing else to say still states what happened.
- **B4.** `title` is the framework's standard title for the status, so a status reads the same whichever part of the server produced it. Only a `400` deviates, carrying the validation title (B1).
- **B5.** Every problem body carries `correlationId`, `traceId` and `instance`, as it does today. `instance` is written as the standard Problem Details member, never as an extension, so the body never has two `instance` keys.
- **B6.** A stale-tag conflict is identified by a documented, stable `type` value, distinct from every other `409`. The client never infers it from a field name.
- **B7.** A `429` keeps today's behaviour: a `Retry-After` header in whole seconds, rounded up, never less than 1, and a `detail` that states the wait when it is known.
- **B8.** No implementation detail reaches a problem body: no validator class names, exception types, constraint or table names. The `errorCode` member is removed, unless Open Question 1 decides to replace it with stable, documented codes.
- **B9.** Every `409` carries a `type` naming the category of conflict it is — a stale tag (B6), a value that already exists, or a constraint that failed in the database. Two categories never share a `type`, so a rule the endpoint decided on is distinguishable from the fallback conflict without comparing message text.

### C. Failure handling is written once

- **C1.** The mapping from an application failure to a status code and problem body exists in exactly one place, and covers every failure category. Adding a category without deciding its status fails to compile, or fails a test.
- **C2.** An endpoint handler contains no failure-handling code. It states how to build its success response; validation and failure conversion happen elsewhere.
- **C3.** Request validation runs automatically, before the handler, for every endpoint whose request type has a validator. A test fails if an endpoint's request type has a registered validator and the endpoint does not run it.
- **C4.** Every problem body, from every source in A1 and B2 plus the rate limiter, the exception handlers and the not-found and method-not-allowed responses, gets its common members (B4, B5) from one shared customisation. No other helper constructs those members.
- **C5.** A validation failure is logged once, centrally, at `Information`, with field names and messages only (A2). Handlers do not log it themselves.

### D. The contract is documented accurately

- **D1.** The OpenAPI document describes every `400` as a validation problem whose `errors` member is required, and every problem schema with `correlationId`, `traceId` and `instance` as required, non-nullable strings.
- **D2.** The OpenAPI document describes the stale-tag `type` value (B6) on the endpoints that can return it.
- **D3.** `EndpointResponseMetadataFixture` asserts the new declarations: a `400` as a validation problem and a `500` as a problem on every API endpoint, a declared `409` as a problem in `application/problem+json`, no success status described as a problem, and no `422`.
- **D4.** `Source/Server/DEVELOPER.md` and any other document describing the error contract match the implementation: no `422` claims, no `.ProducesProblem` on a success status, sample bodies in the new shape, and the handler pattern of C2. The passages that have drifted are corrected in the same change — the customisation snippet that shows `Extensions.Add("correlationId", …)` rather than the indexer, the `429` example showing a fractional `Retry-After`, the `DatabaseExceptionHandler` snippet describing the raw provider message, and the problem-types list that calls `409` entity-tag-only and validation `422`. The decisions B7, B9 and D5 are recorded there too: `Retry-After` is whole seconds and exposed through CORS, database exceptions answer a generic `409` and are logged by the handler, `correlationId` is the identifier to quote to support, and `409` is declared per feature rather than on every write.
- **D5.** Every endpoint that declares a `409` has a test that asserts the body its own rule produced, identified by the `type` of B9, so the test fails if the rule stops running. The fallback conflict carries a different `type` and cannot satisfy it.
- **D6.** Every endpoint that reads a request body declares `413` and `408` as problems, and no endpoint that reads no body declares either. `EndpointResponseMetadataFixture` asserts both directions, so a new body-accepting endpoint cannot omit them and a `GET` cannot claim them.

### E. The client reads every response it receives

- **E1.** On a `400` the client shows every message in `errors`, field and general alike, as it shows every `errorMessage` today.
- **E2.** On a `409` the client shows the stale-tag message ("A conflicting update has been performed by another user. Refresh and try again.") when the body carries the stale-tag `type` (B6), and the body's `detail` for every other conflict.
- **E3.** The client's error response type describes the new shape: `errors` becomes a map of field name to messages, and the `ApiErrorDetail` type disappears along with the `attemptedValue` and `errorCode` members it carried. The client unit tests for error parsing cover the shapes in B1, B3 and B6.
- **E4.** Every other status keeps its current user-facing behaviour.
- **E5.** The client reads every error body it can receive without assuming a problem shape: the problem body of B1 and B3, JSON that is not problem-shaped, the reverse proxy's HTML or plain-text page, an empty body, and a downloaded file whose error body arrives as a blob. A body it cannot interpret is treated as absent and is never rendered into the page.
- **E6.** One normaliser at the interceptor boundary turns each transport failure into a domain error, with a fixed precedence: the problem body's own members first, the message for the status second, a generic message last. It is total — no failure, whatever its body, leaves a message undefined, empty or filled with markup.
- **E7.** A `502`, `503` or `504` that carries no problem body means the application was not reached, and reads as unreachable rather than as a server error with nothing to say. F1 guarantees what the application writes; the proxy answers when the application cannot.
- **E8.** Client unit tests cover each shape in E5 against the statuses that present it — at least a `400`, a `409`, a `500` and a `504` carrying an HTML page, then an empty body and a blob — asserting the user-visible message rather than the parse internals.

### F. Every error is written, whatever the caller asks for

- **F1.** Every error response carries `application/problem+json` and a body, whatever the request's `Accept` header contains. That covers a caller that accepts only `text/html`, the `404` and `405` responses the framework generates, and an exception handled inside the exception handler middleware — which today leaves an empty `500` because writing throws there.
- **F2.** The framework's problem writer is wrapped once at startup so it writes a body it would otherwise decline, and a second registration fails at startup rather than silently replacing the first.
- **F3.** Tests assert the shapes for a caller that accepts only `text/html`: an unmatched route answers `404`, and an unhandled exception answers a generic `500`, both `application/problem+json` with `correlationId` and neither exposing the exception message. No catch-all exception handler is added to achieve it; the framework's handler keeps owning the `500`.

## Out of Scope

- **Moving the entity tag to an `If-Match` header and answering a stale one `412`** — a concurrency-model change of its own, for a future PRD if wanted. While the tag travels in the body, `409` is the correct status: no precondition header was sent, so none can have failed.
- **Replacing AllOverIt's `EnrichedResult` in the application layer** — it would remove the remaining `!` uses, but it touches every service signature. C1 confines the `!` to one place.
- **Rate-limit policy values and partitioning** — unchanged. Only the shape of the `429` body is in scope (B7, C4).
- **Which database exceptions map to which status** — `DatabaseExceptionHandler` keeps its mapping rules. Only how it builds the body is in scope (C4).
- **Localised or machine-readable error codes for the client** — not needed by the current client. If wanted later, it is an additive change, and Open Question 1 records the choice.
- **The health endpoints' failure responses.** A failing `/_health/ready` answers `503` with the health check writer's plain-text `Unhealthy`, not a problem body, and stays that way: the callers are infrastructure and the E2E gate, which read the status code only. The body is written by the health check middleware, so neither `UseStatusCodePages` nor the wrapped problem writer of F2 replaces it. Nothing in B, C or F applies to these two endpoints.
- **The size of the default request body limit** — D6 documents the `413` that Kestrel's 30 MB default produces, but whether 30 MB is the right ceiling for JSON endpoints is a separate decision. See **Related Recommendations**.

## Known Risks and Accepted Limitations

- **A caller that asks for HTML receives JSON.** A browser opening an endpoint URL directly now gets a problem body where the framework would have produced a plain-text page. Accepted: the API is JSON-only and the client never requests HTML.
- **A proxy can answer before the application does.** The reverse proxy returns its own HTML page for a `502` or `504` when the API is unreachable, and a downloaded file answers as a blob, so a client can still receive a body it cannot parse as a problem body. F1 covers only the responses the application writes; the client reads the status instead of the body in those cases (E5, E7).
- **A session already open keeps its bundle.** The app shell and service worker are served `no-cache` and the client checks `version.json`, so a reload picks up a new build — but a tab left open across a server release shows generic messages for `400` and `409` until it reloads. Accepted: the new bodies stay readable to the old bundle, so the page degrades in wording and never fails.
- **Conflict messages may become less specific.** Today the client composes "The 'Description' conflicts with another record that has the same value 'X'" from `propertyName` and `attemptedValue`. Under A1 and E2 it shows the server's `detail` ("The account description already exists") unless Open Question 4 decides otherwise.

## Assumptions

- The React client in `Source/Client/pot-react` is the only consumer of the API. **The client and server ship together**, so the wire contract changes in one release and the client needs no compatibility path for the previous shape.
- FluentValidation remains the validation library.
- The six requests validated with a separate validation context (`Expenses/Create`, `Expenses/Update`, `Incomes/Create`, `Incomes/Update`, `Projections/Get`, `Settings/Upsert`) build that context entirely from the request's own properties. So their validators can derive it themselves, and the automatic validation of C3 needs no per-endpoint context factory. If any context needs data the request does not carry, C3 needs a context hook for that endpoint.
- No client code, end-to-end test or external integration reads `errorCode`, `attemptedValue` or custom-state members. Verified for `src/` (only `propertyName` and `attemptedValue` on a `409`, and `errorMessage` on a `400`, are read). The client unit tests under `tests/api` reference them and will change.

## Decisions Carried Into This Document

All **proposed**, pending confirmation at review.

| Decision                                      | Proposed resolution                                                                                                                                                                                                                                                               |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shape of `errors` on a `400`                  | The framework's validation-problem map (B1), replacing the custom list. Removes the name clash with ASP.NET Core, removes the custom serialisation code and its custom-state camel-casing, and gives the OpenAPI document a known schema. See Open Question 1 for the alternative |
| `errors` on other statuses                    | Removed (B3). Every non-`400` failure has one message, and `detail` carries it                                                                                                                                                                                                    |
| `attemptedValue`                              | Removed from the wire and from logs (A1, A2)                                                                                                                                                                                                                                      |
| `errorCode`                                   | Removed (B8), since nothing reads it and half its values are validator class names. Open Question 1                                                                                                                                                                               |
| How a stale-tag conflict is identified        | A distinct `type` value (B6), the member RFC 9457 defines for identifying a problem type                                                                                                                                                                                          |
| Stale-tag status                              | Stays `409`, because the tag travels in the body (Out of Scope)                                                                                                                                                                                                                   |
| Where validation runs                         | An endpoint filter applied to the API route groups, so a new endpoint is validated without opting in (C3)                                                                                                                                                                         |
| Where failure conversion runs                 | One extension that takes the success-response builder (C1, C2)                                                                                                                                                                                                                    |
| Telling conflicts apart                       | A `type` per conflict category (B9), so a fallback conflict stays distinguishable from one an endpoint's rule decided on, and a test can assert which answered (D5)                                                                                                               |
| `409` declarations                            | Per feature, only where an endpoint's own rule can reject the request. Not declared on every write, because a fallback conflict is a race rather than a normal outcome (D4, D5)                                                                                                   |
| A request whose `Accept` header excludes JSON | Answered with the problem body anyway, by wrapping the framework writer (F1–F3). The API is JSON-only, so no caller loses a format it asked for                                                                                                                                   |
| Unhandled exception                           | The framework's handler keeps writing the `500`; no catch-all handler is added, and a test pins the shape (F3)                                                                                                                                                                    |
| Client and server release                     | Together, in one release, so the client carries no compatibility path for the previous wire shape (Assumptions)                                                                                                                                                                   |
| A body the client cannot read                 | Treated as absent: the status supplies the message and the body is never rendered (E5, E6)                                                                                                                                                                                        |
| A proxy response with no problem body         | Read as the application being unreachable when the status is `502`, `503` or `504` (E7)                                                                                                                                                                                           |

## Open Questions

- **1. Confirm the `errors` shape, and whether codes are wanted.** The proposal is the framework's field-to-messages map, with no error codes. The alternative is to keep the list of objects with `attemptedValue` removed, and either keep `errorCode` with stable, documented values or drop it. The choice changes B1, B8, E1 and E3. Resolved by deciding whether the client will ever branch on a specific rule, rather than on the status. If it will, keep codes, but stable ones, never validator class names.
- **2. Should a rule that depends on current state answer `409` rather than `400`?** "Cannot delete an Account that has linked Expenses" and "linked Incomes" are `ErrorType.Constraint`, answered `400`. The administrator lock-out protections answer `400` too. None of them is wrong in the request itself — the same request succeeds once the state changes — which is what `409` means in RFC 9110. Changing it touches the client's conflict handling (E2) and the user-facing copy. Resolved by a product decision on whether the user should read these as "fix your input" or "this cannot be done right now".
- **3. What URI form should the stale-tag `type` take?** RFC 9457 recommends an absolute URI. A relative reference such as `/problems/stale-entity-tag` is allowed but resolves against the request URI, and a `tag:` URI needs no hosting. Resolved by choosing one form and recording it, since any value is a permanent contract once released.
- **4. Should a duplicate-value conflict quote the value?** A1 forbids echoing submitted values. The server may still compose a non-secret value it chose into its own `detail` ("An account called 'Rent' already exists"). Resolved by deciding whether that specificity is worth a server-side rule about which fields may be quoted.

## Related Recommendations

Found during the same review and **not part of this PRD's scope**. Each is recorded here so it is not lost, with a suggested home. None blocks this PRD.

- **R1. Validate the service provider in every environment.** Nothing calls `UseDefaultServiceProvider`, so `ValidateScopes` and `ValidateOnBuild` are on in Development only, where the framework enables them itself. Elsewhere a singleton holding a scoped service starts cleanly and shares one request's state with every other, and an unconstructable registration surfaces as a `500` on first use. POT is more exposed than most: services and validators are registered by assembly scanning, validators are singletons by default, and exception handlers are always singletons. Turn both on in `Program` and add a startup test under a non-Development environment that a singleton depending on a scoped service fails to start. If enabling them fails startup today, that is a captive dependency already in production, not a reason to leave them off. **Small standalone change.**
- **R2. Transient database faults.** Npgsql is configured with no retry strategy, so a database failover or a dropped connection is a `500` logged at `Error` — through the unhandled path on a read, or `DatabaseExceptionHandler`'s fallback on a write — in the same channel as genuine bugs. Enabling `EnableRetryOnFailure` is not a one-line change: `PotTransactionFactory` opens user-initiated transactions, which the retrying strategy rejects unless each is run inside `Database.CreateExecutionStrategy().ExecuteAsync`, with the whole transaction as the retried unit. Once retries exist, decide whether an exhausted transient fault answers `503` with `Retry-After` rather than `500`; `NpgsqlException.IsTransient` is a usable classifier, and the client cooldown handling from PRD-014 may extend to it. This changes the database exception mapping this PRD leaves alone. **Its own PRD.**
- **R3. Put the user id on every log entry.** `CorrelationIdLogEnricher` adds the correlation id only, so the entry for a `500` says what failed but not who it happened to. Adding the `sub` claim — the id, never a name or an email — to the enricher covers every entry at once. An enricher reads `HttpContext` when each entry is written, so unlike a logging scope it still applies when an exception has unwound to the exception handlers. **Small standalone change.**
- **R4. The default request body limit.** Every JSON endpoint accepts Kestrel's 30 MB default; only the import endpoint sets its own limit. A lower server-wide `MaxRequestBodySize`, with the import endpoint's `RequestSizeLimit` as the exception, bounds what one request can cost. D6 documents the resulting `413` whatever the value. **Small standalone change.**

## Implementation Guidance

For the agent delivering this PRD. Describes the current code and a suggested approach. The requirements above are the contract; this section is advice.

**Never reference this PRD's number, its requirement IDs or this process in code, tests, comments or test names** (prd-discovery skill).

### Current state

| Concern                                            | Where                                                                                                                                                                                                                                             |
| -------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shared customisation (`correlationId`, `instance`) | `Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.AddProblemDetails`, which registers the framework's `IServiceCollection.AddProblemDetails` with a customisation delegate                                                               |
| Application failure → problem body                 | `Pot.AspNetCore/Extensions/EnrichedErrorExtensions.ToProblemDetails`                                                                                                                                                                              |
| Validation result → problem body                   | `Pot.AspNetCore/Concerns/Validation/Extensions/ValidationResultExtensions.ToProblemDetails`                                                                                                                                                       |
| Per-handler validation                             | `Pot.AspNetCore/Concerns/Validation/IProblemDetailsInspector`, `ProblemDetailsInspector`                                                                                                                                                          |
| "Valid" sentinel                                   | `Pot.AspNetCore/Concerns/ProblemDetails/NoProblemDetails`, `Extensions/ProblemDetailsExtensions.IsProblem`                                                                                                                                        |
| Body helper for rate limiter and DB handler        | `Pot.AspNetCore/Concerns/ProblemDetails/ProblemDetailsContextFactory`                                                                                                                                                                             |
| Error entry serialisation                          | `Pot.App/Extensions/ApiDetailsErrorExtensions.GetErrorDetails`                                                                                                                                                                                    |
| Error types                                        | `Pot.App/Errors/*` (`ErrorType`, `ErrorCodes`, `ApiBasicError`, `ApiDetailError`, `ApiDetailErrorCollection`, `ApiDetailErrorFactory`)                                                                                                            |
| Validation logging                                 | `Pot.AspNetCore/Extensions/LoggingExtensions.LogErrors`, `Pot.App/Extensions/LoggerExtensions.LogApiError` (logs the value)                                                                                                                       |
| Other body writers                                 | `Concerns/Middleware/CorrelationIdMiddleware.WriteBadRequestAsync`, `Concerns/ExceptionHandlers/BadRequestExceptionHandler`, `Concerns/ExceptionHandlers/DatabaseExceptionHandler`, `Concerns/RateLimiting/Configuration/RateLimiterOptionsSetup` |
| Framework problem writer                           | `IProblemDetailsWriter` as registered by the framework's `IServiceCollection.AddProblemDetails()`. It declines a request whose `Accept` header names no JSON media type, which is the source of F1                                                |
| Client parsing                                     | `Source/Client/pot-react/src/api/errors/apiErrorResponse.ts`, `src/api/interceptors/axiosInterceptors.ts`, tests under `tests/api/`                                                                                                               |

### Suggested approach

1. **Tests first for A3.** Integration tests for Change Password asserting the body does not contain the submitted password, for a strength failure and for the cross-field rule. Confirm they fail.
2. **One customisation, chained.** Keep the builder extension named `AddProblemDetails`, and move the customisation it passes inline to `builder.Services.AddProblemDetails(options => …)` into an `IConfigureOptions<ProblemDetailsOptions>` setup class, so the builder method only composes registrations. The setup class chains onto any earlier `CustomizeProblemDetails`, then:
   - sets `correlationId` with the indexer, never `Add()`, which throws on an existing key while an error is being written;
   - sets `Instance ??= request path` as the property;
   - gives any `400` that is not already an `HttpValidationProblemDetails` the validation title and an `errors` map, filing `detail` (or a generic message) under the general error key. This one rule satisfies B2 for every route at once.

   Register it before the framework's `builder.Services.AddProblemDetails()`, and register nothing that configures `ProblemDetailsOptions` after it: a later configuration that assigns rather than chains replaces it silently.

3. **Validation as an endpoint filter.** Apply an endpoint filter factory to the API route groups. It finds the handler parameter whose type has a registered validator, runs the validator through the existing `ILifetimeValidationInvoker`, and on failure returns `TypedResults.ValidationProblem(errors)`, keyed by camelCase property name with model-level messages under `""`. Move the six validation contexts into their validators (see **Assumptions**). For C3, add a startup-level test that enumerates registered validators and API endpoints and fails for an endpoint whose request type has a validator the filter did not attach to.
4. **One conversion.** Add `ToHttpResult(this EnrichedResult<T> result, Func<T, IResult> onSuccess)`, plus a non-generic overload if needed, holding the only `!` uses and the only status mapping. A `switch` over `ErrorType` with no default branch, or with `UnreachableException`, makes an unmapped category obvious. Take `detail` from the error's own message: `EnrichedErrorExtensions` copies `EnrichedError.Description` today, which nothing assigns, so it is null on every error a handler reports — carrying it over would give the client nothing to display once B3 removes `errors` from a conflict (B3). Set `type` here for each conflict category (B9); telling a stale tag apart from a duplicate needs the application layer to mark it, for example with a dedicated error type or factory, rather than the client-facing `propertyName`. `DatabaseExceptionHandler` gives a fallback conflict its own `type`, so the two are never confused (D5). Handlers become `return result.ToHttpResult(Response.Created);`.
5. **Retire the helpers.** Delete `IProblemDetailsInspector`, `ProblemDetailsInspector`, `NoProblemDetails`, `ProblemDetailsExtensions`, `ProblemDetailsContextFactory`, `ValidationResultExtensions.ToProblemDetails` (server), `ApiDetailsErrorExtensions`, and `LoggingExtensions.LogErrors`. Make `LogApiError` stop logging the value. Move the rate limiter, `DatabaseExceptionHandler`, `BadRequestExceptionHandler` and `CorrelationIdMiddleware` to plain `ProblemDetails`/`HttpValidationProblemDetails` written through `IProblemDetailsService`, so C4 holds. `ApiDetailError.AttemptedValue` goes whichever shape Open Question 1 chooses, because A1 leaves nothing to put in it: remove the property, the `attemptedValue` parameter from every `ApiDetailErrorFactory` method, and the argument at each call site. `CustomState` goes only if Open Question 1 drops the `errors` list.
6. **Write unconditionally.** Wrap the framework's `IProblemDetailsWriter` so it reports that it can write every request: it writes what the framework writer would have written when that writer accepts the request, and writes the same body itself when it does not, so a caller accepting only `text/html` still receives one (F1). Register it as the only writer and fail at startup if another is already present (F2). Keep it behind the shared customisation so the body still carries `correlationId` (C4).
7. **Document the schema.** Add an `IOpenApiSchemaTransformer` that, for every type assignable to `ProblemDetails`, adds `correlationId` and `traceId` and replaces `instance` with a non-nullable string. It marks all three required, and marks `errors` required on `HttpValidationProblemDetails`. The extensions dictionary is invisible to the generator, which is why these members are missing today. Declare `400` with `ProducesValidationProblem()` rather than `ProducesProblem(400)`, and document each `409` with the `type` of B9. For D6, an endpoint reads a body when it carries `IAcceptsMetadata`, which the framework adds for an inferred or explicit `[FromBody]` parameter; the fixture can select on it. Declare `413` and `408` with `ProducesProblem` per endpoint, or through a helper applied to those endpoints only — not through the helper every endpoint uses. Regenerate `contracts/openapi/v1.json`.
8. **Client.** Update `ApiErrorResponse` to the new shape. `getBadRequestMessage` flattens the `errors` map's values. `getConflictMessage` checks `type` for the stale-tag value and otherwise uses `detail`. Put one normaliser in front of the message getters: it accepts whatever `data` arrives — a problem object, a string page, a `Blob`, `null` — and returns a problem object or an empty one, decoding a blob by its content type so an export failure can be read (E5). The existing `(data as ApiErrorResponse) || {}` cast is the right instinct but is not defensive: a string satisfies the cast. Make it explicit, keep every getter total (E6), and make the `502`/`503`/`504` path say the API was not reached when nothing readable is present (E7). Add unit tests per shape (E8).
9. **Documents.** Correct `DEVELOPER.md` (D4) and regenerate any sample bodies from real responses rather than writing them by hand.
10. **Verify.** Run the full server suite and the client suite, then regenerate `contracts/openapi/v1.json`; the only acceptable outcomes are no change, or a change committed with the code that caused it.

### Reference snippets

The two pieces below are where the framework's behaviour is least obvious and a plausible implementation is subtly wrong. Both are taken from code running against .NET 10 and `Microsoft.AspNetCore.OpenApi` 10.0. They are references, not drop-ins: adapt names, namespaces, guard clauses (`WhenNotNull()`) and logging style to POT's conventions, and let the requirements above win wherever a snippet and a requirement disagree.

**The chained customisation (step 2, C4, B1, B2, B5).**

```csharp
internal sealed class ProblemDetailsOptionsSetup : IConfigureOptions<ProblemDetailsOptions>
{
    internal const string CorrelationIdExtension = "correlationId";
    internal const string ErrorsExtension = "errors";
    internal const string GeneralErrorKey = "";

    // HttpValidationProblemDetails' own title, so a 400 reads the same whichever route produced it.
    private const string ValidationTitle = "One or more validation errors occurred.";

    // For a 400 that arrives with nothing to say, such as UseStatusCodePages filling in a binding failure.
    private const string GeneralValidationError = "The request was not valid.";

    public void Configure(ProblemDetailsOptions options)
    {
        // Chained rather than replaced: a customisation registered earlier still runs, and runs first, so it
        // cannot remove what this one adds. Anything configuring ProblemDetailsOptions AFTER this that assigns
        // rather than chains replaces it outright - register such things before AddProblemDetails().
        var configuredCustomisation = options.CustomizeProblemDetails;

        options.CustomizeProblemDetails = context =>
        {
            configuredCustomisation?.Invoke(context);

            var problemDetails = context.ProblemDetails;

            // The indexer, never Add(): Add() throws on an existing key, and this runs while an error response
            // is being written - where a throw turns a handled failure into an unhandled one.
            problemDetails.Extensions[CorrelationIdExtension] = context.HttpContext.TraceIdentifier;

            // The property, never Extensions["instance"], or the body can carry two "instance" keys.
            problemDetails.Instance ??= context.HttpContext.Request.Path;

            // traceId is added by the framework's writer, from the Activity or the request identifier. Adding it
            // here as well would give one value two sources.

            if (problemDetails.Status == StatusCodes.Status400BadRequest &&
                problemDetails is not HttpValidationProblemDetails)
            {
                problemDetails.Title = ValidationTitle;

                problemDetails.Extensions.TryAdd(ErrorsExtension, new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    [GeneralErrorKey] = [problemDetails.Detail ?? GeneralValidationError],
                });
            }
        };
    }
}
```

```csharp
builder.Services
    .ConfigureOptions<ProblemDetailsOptionsSetup>()
    .AddProblemDetails();
```

**The OpenAPI schema transformer (step 7, D1).** `Microsoft.AspNetCore.OpenApi` 10 uses Microsoft.OpenApi 2.x, where `Type` is the `JsonSchemaType` flags enum and properties are `IOpenApiSchema`; examples written for .NET 9 use types that no longer exist. Register with `options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>()` inside `AddOpenApi`.

```csharp
internal sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Every Problem Details type, not ProblemDetails alone: a 400 is HttpValidationProblemDetails, which the
        // generator describes as a schema of its own.
        if (!typeof(Microsoft.AspNetCore.Mvc.ProblemDetails).IsAssignableFrom(context.JsonTypeInfo.Type))
        {
            return Task.CompletedTask;
        }

        schema.Properties ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);

        schema.Properties["correlationId"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Identifies this request in the logs. Quote it when reporting a problem.",
        };

        schema.Properties["traceId"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Identifies this request in distributed tracing.",
        };

        // Replaced, because the property is nullable on the type and so generated as string-or-null. The
        // customisation always sets it, and "required but nullable" makes a generated client handle a null that
        // never arrives.
        schema.Properties["instance"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "The path of the request that failed.",
        };

        schema.Required ??= new HashSet<string>(StringComparer.Ordinal);
        schema.Required.Add("correlationId");
        schema.Required.Add("traceId");
        schema.Required.Add("instance");

        if (context.JsonTypeInfo.Type == typeof(HttpValidationProblemDetails))
        {
            schema.Required.Add("errors");
        }

        return Task.CompletedTask;
    }
}
```

The `using` directives these need beyond the implicit ones: `Microsoft.Extensions.Options` (the setup class), and `Microsoft.AspNetCore.OpenApi` and `Microsoft.OpenApi` (the transformer).

### Tests that will change

`EndpointResponseMetadataFixture` (the `400` declaration type, the check that a declared `409` is a problem in `application/problem+json`, and the `413`/`408` declarations of D6), `CorrelationAndProblemDetailsFixture` (`detail` strings and `errors` assertions), `DatabaseExceptionHandlerFixture` (`errorCode` assertions), `BadRequestExceptionHandlerFixture`, `ApiDetailsErrorExtensionsFixture` and `ValidationResultExtensionsFixture` (removed or rewritten), `LoggingExtensionsFixture`, the feature fixtures asserting validation bodies, the duplicate-description conflict tests in `Features/Accounts/CreateUpdateFixture`, `Features/Expenses/CreateUpdateFixture` and `Features/Incomes/CreateUpdateFixture` together with the `ShouldBeEntityConflictAsync` helper they use (which assert the field details of a `409` today and must instead assert the conflict `type` of B9, per D5), and the client's `tests/api/errors/apiErrorResponse.test.ts` and `tests/api/interceptors/axiosInterceptors.test.ts`, which also gained cases for an HTML page, an empty body and a blob (E8).

## Manual Test Plan

### Setup

1. Start the full stack locally as described in `Docs/LOCAL-SETUP.md`, and open the client with the browser's developer tools on the Network tab.
2. Sign in as a site administrator, **Admin A**, whose password you know.
3. Create an account named **Rent**.
4. Open **Rent** for editing in two browser tabs, **Tab 1** and **Tab 2**, without saving either.

### A. Submitted values are not echoed

- **A1 — Weak new password.** On Change Password, enter the correct current password and the new password `weakpass1`. Submit.
  Expected: the form shows the strength messages. The `400` response body in the Network tab contains no occurrence of `weakpass1` and no `attemptedValue` member.
- **A2 — New password equal to current.** On Change Password, enter the same valid password in both fields. Submit.
  Expected: the form shows "The new password must be different from the current password." The response body contains neither password.
- **A3 — Server log.** Repeat A1 and read the server console output for that request.
  Expected: the validation entry names the field and message, and does not contain `weakpass1`.

### B. Error shapes

- **B1 — Field validation.** Create an account with an empty description.
  Expected: the form shows the description message. The `400` body has `title` "One or more validation errors occurred.", an `errors` object keyed by `description`, and `correlationId`, `traceId` and `instance`.
- **B2 — Malformed request.** From the developer tools console, send a `POST` to the account-create endpoint with the body `{`.
  Expected: `400`, `application/problem+json`, the validation title, and an `errors` object with a message under the `""` key.
- **B3 — Duplicate name.** Create a second account named **Rent**.
  Expected: `409`. The client shows the server's conflict message. The body has a `detail`, no `errors` member, and the framework title "Conflict".
- **B4 — Stale entity tag.** Change and save **Rent** in **Tab 1**, then change and save it in **Tab 2**.
  Expected: **Tab 2** shows "A conflicting update has been performed by another user. Refresh and try again." The `409` body carries the stale-tag `type` value.
- **B5 — Not found.** Request an account by a random GUID from the console.
  Expected: `404` with title "Not Found", `correlationId` present, and no `errors` member.
- **B6 — Rate limit.** Send rapid anonymous requests to the login endpoint until one is refused.
  Expected: `429` with a whole-number `Retry-After` header of at least 1. The client shows the rate-limited message, and the body's `detail` states the wait.
- **B7 — A caller that asks for HTML.** From the developer tools console, request an account by a random GUID with `Accept: text/html`.
  Expected: `404`, `application/problem+json`, and a body with `correlationId` — no plain-text page and no empty body.
- **B8 — The proxy answers first.** With the Docker stack running (`Source/Docker`), stop only the server container, reload the client, and open a page that reads data.
  Expected: the page states that the server cannot be reached — no blank error, no raw HTML on screen, and navigation keeps working.

### C. Documentation

- **C1 — OpenAPI.** Open `contracts/openapi/v1.json`.
  Expected: `HttpValidationProblemDetails` lists `errors` as required. Both problem schemas list `correlationId`, `traceId` and `instance` as required, non-nullable strings. No operation declares `422`, and no success status references a problem schema. Every operation with a request body declares `413` and `408`, and no `GET` declares either.
- **C2 — Developer guide.** Read the error-handling and endpoint sections of `Source/Server/DEVELOPER.md`.
  Expected: no mention of `422` for validation. Sample bodies match what B1 and B3 returned. The endpoint example declares its success type with `Produces<T>`.

### Cleanup

- Delete the **Rent** account, and reset **Admin A**'s password if A1 or A2 changed it.

### Results

| ID    | Scenario                        | Result | Notes |
| ----- | ------------------------------- | ------ | ----- |
| A1–A3 | Submitted values are not echoed |        |       |
| B1–B8 | Error shapes                    |        |       |
| C1–C2 | Documentation                   |        |       |
