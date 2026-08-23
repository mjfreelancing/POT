---
name: dotnet-integration-test
description: Create or update .NET integration tests with contract-focused assertions. Use when writing integration tests for .NET code, including hosted API tests that exercise HTTP boundary behavior.
license: MIT
---

# .NET Integration Test

Create integration tests for .NET API boundary behavior.

## Unit vs Integration Decision

- Use this skill when tests verify how .NET components interact, including hosted API behavior.
- If the test does not require integration behavior, use the `dotnet_unit_test.prompt.md` prompt instead.
- If tests must boot the application host and validate transport-boundary behavior through real HTTP requests, prefer the `server-integration-test` skill.

## Workflow

1. Place tests under the integration-test project or folder.
2. Assert status code first, then critical contract fields and headers.
3. Use reusable host fixtures for shared setup.
4. Run targeted tests first, then broaden.

## Execution

Adjust the example paths below to match your repository structure.

- `dotnet test .\Source\Server\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` (from `Source/Server`)

## Repository Notes

- Keep repository-specific host fixture and path details in consuming copies.
- Keep shared integration architecture rules in instruction files.
  - POT: Integration tests live under `Source/Server/Pot.AspNetCore.Integration.Tests/**`.
  - POT: Shared host fixtures are in `Source/Server/Pot.AspNetCore.Integration.Tests/Host/`.
