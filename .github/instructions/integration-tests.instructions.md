---
applyTo: "Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs"
---

# Integration Test Instructions

Integration tests live in `Source/Server/Pot.AspNetCore.Integration.Tests/**`.

## Core Rules

### Scope and Ownership

- Keep integration-test work separate from unit-test projects (`Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`).
- Use `Pot.AspNetCore.Integration.Tests` for tests that validate the real ASP.NET Core request pipeline behavior.

### Unit vs Integration Classification

- Use a **unit test** when the test executes a class/method directly (service/repository/utility) without booting the API host.
- Use a **unit test** when dependencies are mocked/faked in-process and assertions focus on local behavior.
- Use an **integration test** when the test boots `Program` via `WebApplicationFactory<Program>` (or derived factory) and sends real HTTP requests through middleware/routing/auth/filters/rate limiting.
- Use an **integration test** when validating endpoint handler behavior and request/response mapping through the hosted API boundary.
- Use an **integration test** when asserting HTTP contracts (status codes, headers, ProblemDetails payload shape, endpoint method contracts such as `405`, CORS behavior, middleware concerns).
- If a test requires `HttpClient` from a test host to verify behavior, it belongs in `Pot.AspNetCore.Integration.Tests`.

### Test Organization and Contracts

- Organize by feature first, then cross-cutting areas (pipeline/security/host).
- Validate API contracts with real HTTP calls through test host factories.
- Assert status code first, then critical contract fields and headers.

### Infrastructure and Assertions

- Use `Host/*WebApplicationFactory.cs` for shared test host configuration.
- Use `WebApplicationFactory<Program>`-style host fixtures and real `HttpClient` calls.
- Prefer typed JSON contract models over manual JSON traversal.
- For repeated integration assertion patterns, create reusable helper extensions in `Host/Extensions` within the integration test project.
- For implementation logging operations under test, configure fake logging in the test host and assert each expected log record using fake logger helpers (`FakeLogCollector`/`FakeLogger`) from `Pot.TestUtils`; assertions should verify the exact rendered message and expected structured logging key/value pairs (including template key like `{OriginalFormat}` when applicable).
- Include validation-failure and method-contract (`405`) checks where applicable.
- For timing metadata (for example `Retry-After`), assert conditionally unless guaranteed.
- Ensure responses do not leak sensitive request/header data.

### Execution Reliability

- For targeted integration fixture/test execution, run from `Source/Server` with:
  - `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`
- Prefer this project-level command over file/path-based discovery tooling for .NET integration tests.
- Use `FullyQualifiedName` filtering for deterministic targeted reruns.

## Expansion Notes

- Add endpoint-family patterns under `Infrastructure and Assertions` with concrete paths.
- Keep long-term integration architecture decisions synchronized with `.github/README.md` and `.github/copilot-workflow-guide.md`.
