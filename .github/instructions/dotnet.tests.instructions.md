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

- Use xUnit naming and structure consistent with the repository.
- Keep assertion style consistent within each test project.
- Keep reusable helpers in a shared test utility project when they are cross-project.
- When setup or assertion patterns repeat, prefer adding or extending shared helpers rather than duplicating logic in individual fixtures.
- Keep shared test utility projects strictly general-purpose; project-specific helpers should remain in the owning test project.
- When internal visibility is needed for testing, use csproj `InternalsVisibleTo` declarations.
- For targeted reruns, prefer project-level execution with `--filter` on fully-qualified test names.

### Placement

- Unit tests: project-level test projects that validate in-process behavior.
- Integration tests: hosted API or end-to-end boundary tests with real HTTP/database boundaries and real `HttpClient` requests.

### Layer-Specific Expectations

- Keep test-only dependencies in test projects; do not add them to production projects.
- Aim for full coverage of behavior and meaningful branch paths for the production code under test; when coverage gaps are discovered, add targeted tests to close them.

## Expansion Notes

- Keep framework/tooling choices in project-level docs if needed.
  - POT: Repository test projects are `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, and `Pot.AspNetCore.Integration.Tests`.
  - POT: Current assertion stack uses Shouldly with NSubstitute.
  - POT: Follow `*Fixture.cs` naming with descriptive method names.
  - POT: Shared helpers live in `Pot.TestUtils` when they are genuinely cross-project.
- Add layer-specific conventions in dedicated scoped files.
  - POT: Use `Pot.App.Tests`, `Pot.Data.Tests`, or `Pot.AspNetCore.Tests` for in-process unit tests.
  - POT: Use `Pot.AspNetCore.Integration.Tests` with `WebApplicationFactory<Program>` and real `HttpClient` requests for hosted API tests.
  - POT: For targeted integration reruns from `Source/Server`, use `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`.
  - POT: App-layer tests validate `EnrichedResult` success and failure semantics.
  - POT: Data-layer tests validate specification and query behavior without leaking API-layer concerns.
