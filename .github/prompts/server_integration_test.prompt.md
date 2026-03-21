---
name: server_integration_test
description: Create or update API integration tests under Source/Server/Pot.AspNetCore.Integration.Tests/** with contract-focused assertions.
---

Create integration tests for API behavior.

## Scope

- Target integration tests under `Source/Server/Pot.AspNetCore.Integration.Tests/**`.
- Use when test behavior must be verified through hosted API + real HTTP requests.

## Unit vs Integration Decision

- Use this prompt when tests boot `Program` through `WebApplicationFactory<Program>` and assert HTTP boundary behavior.
- Integration scenarios include middleware, CORS, rate limiting, security headers, ProblemDetails contract shape, and endpoint method contracts (`405`).
- If the test does not require hosted API + `HttpClient`, use `server_unit_test.prompt.md` instead.

## Workflow

1. Place tests in feature/cross-cutting folders under `Source/Server/Pot.AspNetCore.Integration.Tests` (for example `Features/*`, `Pipeline/*`, `Security/*`, `Host/*`).
2. Follow scoped integration instruction file for contract/assertion standards.
3. Add contract-focused scenarios (success/failure/method/validation as applicable).
4. Run narrow scope first, then broaden.

## Execution

- Run integration project scope first:
  - `dotnet test .\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` (from `Source/Server`)

## Expansion Notes

- Keep integration architecture rules in `.github/instructions/aspnetcore.integration-tests.instructions.md`.
