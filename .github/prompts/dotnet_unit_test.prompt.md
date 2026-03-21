---
name: dotnet_unit_test
description: Create or update .NET unit tests with deterministic setup and focused assertions.
---

Create unit tests for .NET code.

## Workflow

1. Place tests in the nearest unit-test project for the changed code.
2. Follow scoped instruction files for style and assertion conventions.
3. Add deterministic setup and behavior-focused assertions.
4. Run smallest relevant test scope first, then broaden.

## Execution

Adjust the example paths below to match your repository structure.

- `dotnet test .\Source\Server\MySolution.sln -c Debug --nologo --verbosity minimal` (from `.\Source\Server`)
