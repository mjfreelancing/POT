---
applyTo: "Source/Server/*Tests/**/*.cs"
---

# .NET Test Instructions

## Core Rules

### Scope and Isolation

- Keep tests deterministic and isolated from external systems.
- Prefer behavior and contract assertions over implementation details.
- Keep unit and integration tests separated by project responsibility.
- Keep tests in the nearest layer-specific test project.

### Conventions

- Server tests use xUnit across `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, and `Pot.AspNetCore.Integration.Tests`.
- Assertion style currently follows Shouldly with NSubstitute test doubles.
- Follow existing naming style in the repo (`*Fixture.cs` and descriptive method names).
- Keep assertion style consistent within each test project.
- Keep reusable helpers in `Pot.TestUtils` when they are genuinely cross-project.
- When setup or assertion patterns repeat, prefer adding or extending helpers in `Pot.TestUtils` rather than duplicating logic in individual fixtures.
- Keep `Pot.TestUtils` strictly general-purpose; project-specific helpers should remain in the owning test project.
- When internal visibility is needed for testing, use csproj `InternalsVisibleTo` declarations such as `<InternalsVisibleTo Include="Pot.App.Tests" />`.

### Placement

- Place tests in `Pot.App.Tests`, `Pot.Data.Tests`, or `Pot.AspNetCore.Tests` when validating a single class, service, or component with in-process test doubles and no hosted API.
- Place integration tests in `Pot.AspNetCore.Integration.Tests` when validating request pipeline behavior via `WebApplicationFactory<Program>` and real `HttpClient` requests.

### Layer-Specific Expectations

- For App-layer tests, validate `EnrichedResult` success and failure semantics rather than exception-only assertions.
- For Data-layer tests, validate specification and query behavior without leaking API-layer concerns.
- Keep test-only dependencies in test projects; do not add them to production projects.

## Commands

Run in `Source/Server`:

- `dotnet test`
- `dotnet test --collect:"XPlat Code Coverage"`
- For targeted .NET test runs, execute the owning test project directly with `--filter "FullyQualifiedName~<FixtureOrTestName>"`.
- For integration fixtures, use:
  - `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`

## Expansion Notes

- Keep framework or tooling choices in project-level docs if needed.
- Add layer-specific conventions in dedicated scoped files only when they are not already covered here.
