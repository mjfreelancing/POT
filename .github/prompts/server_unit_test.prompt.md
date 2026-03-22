---
name: server_unit_test
description: Create or update server unit tests in the nearest test project.
---

Create unit tests for server code.

## Scope

- Target server unit tests only.
- Place tests in the nearest server unit-test project for the changed code.
- Do not place hosted API or end-to-end boundary tests here.

## Workflow

1. Identify the owning layer or project and place tests in the nearest server unit-test project.
2. Follow scoped instruction files for coding and assertion conventions.
3. Add deterministic setup and behavior-focused assertions.
4. Run the smallest relevant test scope first, then broaden when useful.

## Execution

Adjust the example paths below to match your repository structure.

- `dotnet test .\Source\Server\My.Project.Tests\My.Project.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal` (from `.\Source\Server`)
- `dotnet test .\Source\Server\MySolution.sln -c Debug --nologo --verbosity minimal` (from `.\Source\Server`)

## Expansion Notes

- Add repository-specific project paths and command variants in consuming copies.
  - POT: Nearest server unit-test projects are `Pot.App.Tests`, `Pot.Data.Tests`, and `Pot.AspNetCore.Tests`.
  - POT: Hosted API tests using `WebApplicationFactory<Program>` and `HttpClient` belong in `Pot.AspNetCore.Integration.Tests`.
  - POT: Common commands from `Source/Server` are `dotnet test Pot.App.Tests/Pot.App.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal`, `dotnet test Pot.Data.Tests/Pot.Data.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal`, `dotnet test Pot.AspNetCore.Tests/Pot.AspNetCore.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal`, and `dotnet test pot.sln -c Debug --nologo --verbosity minimal`.
- Keep cross-project placement rules in `Scope`.
