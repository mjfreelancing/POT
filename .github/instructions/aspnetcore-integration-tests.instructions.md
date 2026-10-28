---
description: "Hosted API integration test conventions"
applyTo: "Source/Server/Pot.AspNetCore.Integration.Tests/**"
---

# Hosted API integration tests

General .NET test conventions (naming, xUnit v3, Verify) are in `.github/instructions/dotnet-tests.instructions.md`.

Rerun targeted tests from `Source/Server`:
`dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`. Start targeted, then broaden; prefer this over file-path-based discovery.

## Unit or integration?

- Unit test: executes a class or method directly without booting the API host, or mocks/fakes dependencies in-process and asserts local behavior.
- Integration test (belongs here): boots `Program` via `WebApplicationFactory<Program>` (or a derived factory) and sends real HTTP requests through middleware, routing, auth, filters or rate limiting; validates endpoint handler behavior and request/response mapping; or asserts HTTP contracts (status codes, headers, ProblemDetails shape, CORS, `405` method contracts).

## Reading the messages the application sends

The test host removes every hosted service, so nothing consumes the email channel and no message leaves the process. It replaces the channel writer with `CapturingEmailWriter`, which keeps every submitted message so a test can read what a person would have received.

- Read a message with `Emails` (on `IntegrationAuthFixtureBase`) — `Latest(type, recipient)` for the most recent message of a type to an address, `Snapshot()` for everything in submission order, `Clear()` to ignore what came before.
- Read a _secret_ only through `GetInvitationPassword(recipient)`. A test never parses a message body or reads a field to find a password; if a new message type carries a secret a scenario needs, add a helper beside that one.
- A missing message throws with the messages that were captured, so a provisioning problem reads as itself rather than as a null reference in the test that follows.
- Never register a capturing writer outside the test host, and never add a way for the running product to record a message. `MessageCaptureFixture` asserts the shipped assembly offers exactly one writer and that the capture lives in this project.

## Provisioning a scenario

A scenario is a tenant and the people in it, declared as data and provisioned by the product's own routes, so the flows a person uses are the flows under test.

- Derive a scenario fixture from `ScenarioFixtureBase` (not `IntegrationAuthFixtureBase` directly). Declare the tenant with `ScenarioDefinition` and `ScenarioMember`, then call `ProvisionScenarioAsync`. It returns the site, the owner and every member with the credentials that let a test act as them.
- Do not seed a tenant a scenario could reach instead. Signing up, approving, inviting, activating an invitation and changing a status all happen through endpoints; the harness reads the sign-up code from the store and each password from the message that carried it.
- A state no route can produce is declared as `ScenarioDirectWrite` and must state the reason in the same sentence a reviewer would ask for. A blank reason fails at the validation step, so the exception stays visible rather than becoming habit.
- Every identifier is unique per provisioning, so two scenarios may share a name and tests may run in any order.
- A provisioning failure raises `ScenarioProvisioningException`, which names the step and carries the status and body the route answered with. Never catch it to keep a test going: a broken harness must read as a broken harness, not as a failing assertion about behaviour.
- `ScenarioFixtureBase` raises the host's rate limits for its own fixture, because provisioning spends several anonymous requests before the first assertion. Every other fixture keeps the production limits, and `HeadersAndRateLimitFixture` continues to prove them.

## The endpoint inventory and the authorization matrix

Every documented operation is exercised for authorization by a generated case. Nothing about coverage is written per operation, so an endpoint added later is exercised before anyone remembers to test it.

- The inventory is the generated contract, read from a running host with `DocumentedOperations`. Never keep a list of operations beside the tests: a list is what goes stale.
- `AuthorizationMatrix.Generate` produces the cases: each operation anonymously, as the administrator role and as the viewer role, plus a case naming another tenant's row when the path contains `{id}`.
- The expectation comes from the endpoint, not from a table of intended outcomes: `EndpointRequirements` reads each mapped route's `AllowAnonymous` marker and policies, and `EndpointRequirement.Allows` decides whether a caller's permissions satisfy them. A permission policy carries the permission string, so a caller's permissions are resolved with the application's own `IPermissionService` rather than guessed from the role name.
- A case asserts the authorization decision, not a business outcome: 401 when the endpoint refuses anonymous callers, 403 when the caller lacks the declared permission, and neither when the declaration lets the caller through. Ids in a path are replaced with an identifier that exists nowhere, unless the case is about another tenant's row.
- A case naming another tenant's row requires the row to exist, so `ForeignTenantRows` classifies the resource a path addresses. A path naming a row of an unclassified resource fails the coverage check rather than quietly losing its case.
- An operation may only be ignored through `IgnoredOperations`, with a reason a reviewer can judge, and the list stays small. A reason is acceptable when the case cannot be run safely or means nothing: the operation ends the session, the tenant or the identity the run depends on, its answer is decided by something other than the caller's authorization, or the only body it accepts would change data. "Not written yet" is not a reason.
- `InventoryCoverage` decides whether coverage is complete, and its rules are tested on operations the application does not contain, so the failure modes stay proven while the contract happens to be fully covered.

## Conventions

- Organize by feature first, then cross-cutting concerns. Order tests to follow the endpoint/handler flow (early validation or logging paths before deeper branches).
- Assert HTTP status first, then critical contract fields and headers. Prefer typed response models over ad-hoc JSON traversal.
- Shared host configuration lives in `Host/*WebApplicationFactory.cs`; use real `HttpClient` calls. Put repeated assertion patterns in `Host/Extensions`.
- Assert logs with `FakeLogCollector` from the test host (`GetFakeLogCollector()`, `Microsoft.Extensions.Diagnostics.Testing`); extract repeated log assertions into shared helpers. Do not use `Pot.TestUtils` for this.
- Include validation-failure and method-contract (`405`) checks where applicable, and verify responses do not leak sensitive request or header data.

