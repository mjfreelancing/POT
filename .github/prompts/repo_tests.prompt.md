---
name: repo_tests
description: Run repository test suites with project defaults, then summarize failures.
---

Run tests using repository defaults.

## Workflow

1. Start with the smallest relevant scope, then broaden.
2. Summarize root failures before applying fixes.
3. Broaden only when needed.

## Commands

Adjust the example paths below to match your repository structure.

- .NET: `dotnet test .\Source\Server\MySolution.sln -c Debug --nologo --verbosity minimal` (from `.\Source\Server`)
- Typescript: `npm run test` (from `.\Source\Client\my-app`)

## Execution Rules

1. Default to full suite execution unless the user asks for a narrower scope.
2. If tests fail, summarize failing tests and root messages before proposing or applying fixes.
3. If fixes are requested, proceed immediately after the failure summary.
4. If tests pass, return concise totals and stop.

## Expansion Notes

- Add repository-specific entry points under `Commands` while keeping this prompt's examples generic.
  - POT: Full server suite is `dotnet test pot.sln -c Debug --nologo --verbosity minimal` from `Source/Server`.
  - POT: Server project-only runs use `dotnet test <path-to-project.csproj> -c Debug --nologo --verbosity minimal` from `Source/Server`.
  - POT: Server integration tests use `dotnet test .\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` from `Source/Server`.
  - POT: Client suite runs use `npm run test` from `Source/Client/pot-react`.
- Keep coding standards in instruction files, not in this prompt.
