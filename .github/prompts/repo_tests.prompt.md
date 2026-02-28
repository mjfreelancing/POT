---
name: repo_tests
description: Run client/server tests with POT defaults, then summarize failures or proceed to fixes.
---

Run tests using repo commands.

## Scope

- Run client/server test commands for this repository.
- Follow scoped instruction files for coding and assertion conventions when fixes are requested.

## Workflow

- Start with the smallest relevant scope, then broaden.
- Summarize root failures before applying fixes.

## Commands

- Full server suite (default):
  `dotnet test pot.sln -c Debug --nologo --verbosity minimal` (run from `Source/Server`)
- Server project only:
  `dotnet test <path-to-project.csproj> -c Debug --nologo --verbosity minimal` (run from `Source/Server`)
- Server integration tests:
  `dotnet test .\Pot.AspNetCore.Integration.Tests\Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` (run from `Source/Server`)
- Client suite:
  `npm run test` (run from `Source/Client/pot-react`)

## Execution Rules

1. Default to full server suite unless user asks for a narrower scope.
2. If tests fail, summarize failing tests and root messages first.
3. If user asked to fix, proceed immediately after summary.
4. If tests pass, return concise totals.

## Expansion Notes

- Add new test entry points under `Commands` with exact working directory context.
- Keep behavior rules under `Execution Rules` and avoid duplicating coding standards from instruction files.
