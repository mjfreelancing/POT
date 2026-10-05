---
name: run-tests
description: Run the POT server and/or client test suites (unit and E2E) and summarize failures. Use when asked to run tests, check the build is green, or verify a change.
argument-hint: "[server|client|e2e|all] [project or filter]"
---

# Run Tests

Two kinds of run are in scope, and they are not interchangeable:

- **Unit tests** are fast and run for every affected change: xUnit for the server, Vitest for the client.
- **E2E tests** are slow Playwright user journeys that boot the full stack. Run them deliberately (preflight, then the dev suite or the prodlike gate), never as routine verification.

Start with the smallest relevant scope, then broaden. Scope from the user input if given; otherwise run the full suite for the affected side.

## Where to run from

Every command runs from its own project directory. Running from the repository root does not resolve the projects (client commands prompt to install Vitest instead).

| Side   | Working directory         |
| ------ | ------------------------- |
| Server | `Source/Server`           |
| Client | `Source/Client/pot-react` |

## Unit tests

### Server (xUnit) - from `Source/Server`

| Scope              | Command                                                                                                             |
| ------------------ | ------------------------------------------------------------------------------------------------------------------- |
| Full server        | `dotnet test pot.sln -c Debug --nologo --verbosity minimal`                                                         |
| One server project | `dotnet test <Project>/<Project>.csproj -c Debug --nologo --verbosity minimal`                                      |
| Targeted server    | add `--filter "FullyQualifiedName~<FixtureOrTestName>"` to a project command                                        |
| Server integration | `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal` |

### Client (Vitest) - from `Source/Client/pot-react`

| Scope       | Command           |
| ----------- | ----------------- |
| Client unit | `npm run test`    |
| Interactive | `npm run test:ui` |

## E2E tests (Playwright) - from `Source/Client/pot-react`

E2E specs live under `e2e/`; read `.github/instructions/e2e.instructions.md` before running.

Always run the read-only preflight first. It reports anything holding the E2E ports and exits non-zero when a run would be blocked:

`npm run e2e:preflight`

| Scope                        | Command                                                                                                  | When                                       |
| ---------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| Dev suite (full matrix)      | `npm run e2e:all:dev`                                                                                    | Opt-in for a change to client product code |
| Dev suite, keep evidence     | `npm run e2e:all:dev:log`                                                                                | When the run output must be retained       |
| Single project / subset      | `npm run e2e` (or `npm run e2e:smoke`, `npm run e2e:chromium`, `npm run e2e:edge`, `npm run e2e:mobile`) | Quick local checks                         |
| Prodlike suite (full matrix) | `npm run e2e:all:prodlike`                                                                               | Completion gate for client-affecting work  |
| Prodlike, on demand          | `npm run e2e:prodlike`                                                                                   | When the client production build changes   |
| Open last HTML report        | `npm run e2e:report`                                                                                     | After a failing run                        |

The dev suite drives the Vite dev server; the prodlike suite drives the production build (`playwright.prod.config.ts`). Do not run the prodlike suite per change.

## Rules

1. If tests fail, summarize the failing tests and root messages before proposing or applying fixes.
2. If the user asked for fixes, proceed right after that summary.
3. If tests pass, return concise totals and stop.
4. Never pass `--reporter` to a Playwright script; the config governs the reporters.
5. Treat E2E as opt-in: confirm before starting a suite, run the preflight first, and report failures before changing anything.
