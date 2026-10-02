---
name: dotnet-unit-test
description: Create or update .NET unit tests for server code with deterministic setup and focused assertions. Use for Pot.App, Pot.Data, Pot.AspNetCore and Pot.Shared tests, not hosted API tests.
argument-hint: "[class or file under test]"
---

# .NET Unit Test

Conventions: `.github/instructions/dotnet-tests.instructions.md`. Hosted API / end-to-end boundary tests do not belong here; use `server-integration-test`.

1. Identify the owning project and place tests in its unit-test project: `Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests` or `Pot.Shared.Tests` (under `Source/Server`).
2. Follow the fixture, naming and assertion conventions in the rule file.
3. Use deterministic setup and behavior-focused assertions.
4. Run the smallest scope first, from `Source/Server`:
   `dotnet test <Project>.Tests/<Project>.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>" --nologo --verbosity minimal`
5. Broaden if useful: `dotnet test pot.sln -c Debug --nologo --verbosity minimal`.
