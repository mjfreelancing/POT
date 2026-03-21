---
applyTo: "Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs"
---

# ASP.NET Core Integration Test Instructions

Integration tests live in `Source/Server/Pot.AspNetCore.Integration.Tests/**`.

## Core Rules

### Classification

- Use a unit test when the test executes a class or method directly without booting the API host.
- Use a unit test when dependencies are mocked or faked in-process and assertions focus on local behavior.
- Use an integration test when the test boots `Program` via `WebApplicationFactory<Program>` (or derived factory) and sends real HTTP requests through middleware, routing, auth, filters, or rate limiting.
- Use an integration test when validating endpoint handler behavior and request/response mapping through the hosted API boundary.
- Use an integration test when asserting HTTP contracts such as status codes, headers, ProblemDetails payload shape, CORS behavior, and endpoint method contracts such as `405`.

### Organization and Assertions

- Organize tests by feature first, then cross-cutting concerns.
- Assert HTTP status first, then critical contract fields and headers.
- Prefer typed response models over ad-hoc JSON traversal.
- Validate API contracts with real HTTP calls through test host factories.

### Infrastructure and Assertions

- Use `Host/*WebApplicationFactory.cs` for shared test host configuration.
- Use `WebApplicationFactory<Program>`-style host fixtures and real `HttpClient` calls.
- For repeated integration assertion patterns, create reusable helper extensions in `Host/Extensions` within the integration test project.
- For implementation logging operations under test, configure fake logging in the test host and assert expected log records using `FakeLogCollector` or `FakeLogger` helpers from `Pot.TestUtils`.
- Include validation-failure and method-contract (`405`) checks where applicable.
- Ensure responses do not leak sensitive request or header data.

### Reliability

- Start with targeted fixture or test execution, then broaden scope.
- Keep shared host setup in reusable factory fixtures.
- For targeted integration fixture or test execution, run from `Source/Server` with:
  - `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`
- Prefer this project-level command over file-path-based discovery tooling for deterministic reruns.

## Expansion Notes

- Keep endpoint-family patterns in this file.
- Keep generic unit-test rules in `dotnet.tests.instructions.md`.
