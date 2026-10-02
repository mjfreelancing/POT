---
name: run-tests
description: Run the POT server and/or client test suites and summarize failures. Use when asked to run tests, check the build is green, or verify a change.
argument-hint: "[server|client|all] [project or filter]"
---

# Run Tests

Start with the smallest relevant scope, then broaden. Scope from the user input if given; otherwise run the full suite for the affected side.

| Scope | Command (run from) |
| --- | --- |
| Full server | `dotnet test pot.sln -c Debug --nologo --verbosity minimal` (`Source/Server`) |
| One server project | `dotnet test <Project>/<Project>.csproj -c Debug --nologo --verbosity minimal` (`Source/Server`) |
| Targeted server | add `--filter "FullyQualifiedName~<FixtureOrTestName>"` |
| Server integration | `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` (`Source/Server`) |
| Client | `npm run test` (`Source/Client/pot-react`) |

Rules:

1. If tests fail, summarize the failing tests and root messages before proposing or applying fixes.
2. If the user asked for fixes, proceed right after that summary.
3. If tests pass, return concise totals and stop.
