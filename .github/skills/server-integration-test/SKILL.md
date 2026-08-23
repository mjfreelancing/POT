---
name: server-integration-test
description: Create or update hosted server API integration tests with contract-focused assertions. Use when tests must boot the application host and validate transport-boundary behavior through real HTTP requests.
license: MIT
---

# Server Integration Test

Create integration tests for server API boundary behavior.

## Scope

- Target hosted API integration tests that verify behavior through real HTTP requests.
- Use this skill when the test must boot the application host and validate transport-boundary behavior.

## Unit vs Integration Decision

- Use this skill when tests require the hosted server, middleware, or real `HttpClient` requests.
- If the test does not require hosted API behavior, use the `dotnet_unit_test.prompt.md` prompt instead.
- For general .NET integration tests that do not require booting the application host, use the `dotnet-integration-test` skill instead.

## Workflow

1. Place tests under the integration-test project or folder.
2. Assert status code first, then critical contract fields, headers, and error shape as needed.
3. Use reusable host fixtures for shared setup.
4. Run targeted tests first, then broaden.

## Execution

Adjust the example path below to match your repository structure.

- `dotnet test .\Source\Server\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` (from `Source/Server`)

## Repository Notes

- Keep repository-specific host fixture and path details in consuming copies.
- Keep shared integration architecture rules in instruction files.
  - POT: Integration tests live under `Source/Server/Pot.AspNetCore.Integration.Tests/**`.
  - POT: Shared host fixtures are in `Source/Server/Pot.AspNetCore.Integration.Tests/Host/`.
  - POT: For targeted reruns, use `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"` from `Source/Server`.
