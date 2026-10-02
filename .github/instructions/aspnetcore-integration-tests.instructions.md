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

## Conventions

- Organize by feature first, then cross-cutting concerns. Order tests to follow the endpoint/handler flow (early validation or logging paths before deeper branches).
- Assert HTTP status first, then critical contract fields and headers. Prefer typed response models over ad-hoc JSON traversal.
- Shared host configuration lives in `Host/*WebApplicationFactory.cs`; use real `HttpClient` calls. Put repeated assertion patterns in `Host/Extensions`.
- Assert logs with `FakeLogCollector` from the test host (`GetFakeLogCollector()`, `Microsoft.Extensions.Diagnostics.Testing`); extract repeated log assertions into shared helpers. Do not use `Pot.TestUtils` for this.
- Include validation-failure and method-contract (`405`) checks where applicable, and verify responses do not leak sensitive request or header data.
