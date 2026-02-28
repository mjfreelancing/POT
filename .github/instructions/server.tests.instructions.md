---
applyTo: "Source/Server/*Tests/**"
---

# Server Test Instructions

## Core Rules

### Scope and Isolation

- Keep tests in the nearest layer-specific test project.
- Keep tests deterministic and isolated from external systems.
- Prefer behavior and contract assertions over implementation details.
- Keep unit tests and integration tests separated by project responsibility.

### Project Conventions

- Server tests use xUnit across `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, and `Pot.AspNetCore.Integration.Tests`.
- Assertion style currently follows Shouldly (with NSubstitute test doubles).
- Follow existing naming style in the repo (`*Fixture.cs` and descriptive method names).
- Consider `Pot.TestUtils` for reusable test helpers/extensions shared across test projects.
- When setup/assertion patterns repeat, prefer adding or extending helpers in `Pot.TestUtils` rather than duplicating logic in individual fixtures.
- When internal visibility is needed for testing, use this approach in the csdproj `<InternalsVisibleTo Include="Pot.App.Tests" />` to expose to the relevant test project.

### Unit vs Integration Placement

- Place tests in `Pot.App.Tests`, `Pot.Data.Tests`, or `Pot.AspNetCore.Tests` when validating a single class/handler/service with in-process test doubles and no hosted API.
- Place tests in `Pot.AspNetCore.Integration.Tests` when validating request pipeline behavior via `WebApplicationFactory<Program>` + real `HttpClient` requests.
- Integration concerns include middleware, rate limiting, CORS, security headers, ProblemDetails contracts, and endpoint method contracts (`405`).
- If a test needs to assert behavior at HTTP boundary level (status code/headers/body contract), it is an integration test.

### Layer-Specific Expectations

- For App-layer tests, validate `EnrichedResult` success/failure semantics rather than exception-only assertions.
- For Data-layer tests, validate specification/query behavior without leaking API-layer concerns.
- Keep test dependencies in test projects; do not add test-only packages to production projects.

## Commands

Run in `Source/Server`:

- `dotnet test`
- `dotnet test --collect:"XPlat Code Coverage"`

Coverage helper:

- `./code_coverage.ps1`
- Coverage prompt: `.github/prompts/server_coverage.prompt.md`

## Expansion Notes

- Add new test guidance under the matching subsection (`Scope`, `Project Conventions`, or `Layer-Specific`).
- When introducing a new test helper convention, include the owning test project path.
- Keep assertion-framework guidance centralized here to avoid drift across other docs.
