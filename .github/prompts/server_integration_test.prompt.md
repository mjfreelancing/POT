---
name: server_integration_test
description: Create or update hosted API integration tests with contract-focused assertions.
---

Create integration tests for server API boundary behavior.

## Scope

- Target hosted API integration tests that verify behavior through real HTTP requests.
- Use this prompt when the test must boot the application host and validate transport-boundary behavior.

## Unit vs Integration Decision

- Use this prompt when tests require the hosted server, middleware, or real `HttpClient` requests.
- If the test does not require hosted API behavior, use `server_unit_test.prompt.md` instead.

## Workflow

1. Place tests under the integration-test project or folder.
2. Assert status code first, then critical contract fields, headers, and error shape as needed.
3. Use reusable host fixtures for shared setup.
4. Run targeted tests first, then broaden.

## Execution

Adjust the example path below to match your repository structure.

- `dotnet test .\Source\Server\My.Api.Integration.Tests\My.Api.Integration.Tests.csproj --nologo --verbosity minimal` (from `.\Source\Server`)

## Expansion Notes

- Keep repository-specific host fixture and path details in consuming copies.
  - POT: Integration tests live under `Source/Server/Pot.AspNetCore.Integration.Tests/**`.
  - POT: Organize tests under folders such as `Features/*`, `Pipeline/*`, `Security/*`, and `Host/*`.
  - POT: Common scenarios include middleware, CORS, rate limiting, security headers, `ProblemDetails`, and `405` method contracts.
  - POT: Run `dotnet test .\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` from `Source/Server`.
- Keep shared integration architecture rules in instruction files.
