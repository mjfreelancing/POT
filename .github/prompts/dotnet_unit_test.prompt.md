---
name: dotnet_unit_test
description: Create or update .NET unit tests with deterministic setup and focused assertions.
---

Create unit tests for .NET code.

## Scope

- Place tests in the nearest unit-test project for the changed code.
- Do not place hosted API or end-to-end boundary tests here.

### Server code

- Target server unit tests only.
- Place tests in the nearest server unit-test project for the changed code.

## Workflow

1. Identify the owning layer or project and place tests in the nearest unit-test project.
2. Follow scoped instruction files for coding and assertion conventions.
3. Add deterministic setup and behavior-focused assertions.
4. Run the smallest relevant test scope first, then broaden when useful.

## Execution

Adjust the example paths below to match your repository structure.

- `dotnet test .\Source\Server\My.Project.Tests\My.Project.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal` (from `.\Source\Server`)
- `dotnet test .\Source\Server\MySolution.sln -c Debug --nologo --verbosity minimal` (from `.\Source\Server`)

## Expansion Notes

- Keep coding and assertion conventions in instruction files.
- Keep cross-project placement rules in `Scope`.
- Add repository-specific project paths and command variants in consuming copies.
  - POT: Unit-test projects are `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, and `Pot.Shared.Tests` under `Source/Server`.
  - POT: Target a project with `dotnet test .\<Project>.Tests\<Project>.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal` from `Source/Server`.
  - POT: Full server suite is `dotnet test pot.sln -c Debug --nologo --verbosity minimal` from `Source/Server`.
