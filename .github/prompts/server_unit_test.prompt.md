---
name: server_unit_test
description: Create or update POT server unit tests.
model: GPT-5.3-Codex (copilot)
---

Create unit tests for server code.

## Scope

- Target server unit tests only.
- Place tests in the nearest server unit-test project for the changed code (`Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`).
- Do not place `WebApplicationFactory<Program>` + `HttpClient` tests here; those belong in `Pot.AspNetCore.Integration.Tests`.

## Workflow

1. Identify target layer and place tests in the nearest server test project.
2. Follow scoped instruction files for coding/test conventions.
3. Add/adjust tests with deterministic setup and behavior-focused assertions.
4. Run smallest relevant test scope first, then broaden when useful.
5. Keep C# method/constructor parameter lists compact (avoid one-parameter-per-line formatting unless wrapping is truly needed).

## Execution

- Run relevant test scope first, then broaden as needed:
  - `dotnet test pot.sln -c Debug --nologo --verbosity minimal` (from `Source/Server`)

## Expansion Notes

- Add additional unit-test commands under `Execution` with explicit working directory context.
- Keep cross-project test-placement rules in `Scope`.
