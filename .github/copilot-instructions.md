# POT Copilot Instructions (Global)

POT is projection-first: optimize for future cash-flow projections, not historical budgeting.

Instruction authoring model:

- Keep standards concrete and repository-specific.
- Keep prompts workflow-focused and avoid duplicating instruction rules.
- Keep implementation details in path-scoped instruction files.

## Required setting

- `.vscode/settings.json` sets `github.copilot.chat.codeGeneration.useInstructionFiles: true`; without it, Copilot may not reliably load the scoped instruction files below.

Use path-scoped instructions for implementation details. The authoritative `applyTo` scope for each file is its own front matter; see `.github/instructions/README.md` for the catalog.

- Language-agnostic baseline: `.github/instructions/language-agnostic-core.instructions.md`
- ASP.NET Core API baseline: `.github/instructions/aspnetcore-api.instructions.md`
- C# scope: `.github/instructions/csharp.instructions.md`
- Coding patterns and AllOverIt preferences: `.github/instructions/coding-patterns.instructions.md`
- .NET test baseline: `.github/instructions/dotnet.tests.instructions.md`
- ASP.NET Core integration tests scope: `.github/instructions/aspnetcore.integration-tests.instructions.md`
- Postgres/EF Core baseline: `.github/instructions/postgres-efcore.instructions.md`
- React client scope: `.github/instructions/react-client.instructions.md`
- TypeScript scope: `.github/instructions/typescript.instructions.md`
- Client tests scope: `.github/instructions/client.tests.instructions.md`
- Playwright E2E scope: `.github/instructions/playwright-e2e.instructions.md`
- Docker scope: `.github/instructions/docker.instructions.md`

## Shared Integration Workflows

- Preferred full-stack startup in VS Code: task `docker-start-client-server`; stop with `docker-stop-client-server`.
- Docker compose reference: `Source/Docker/docker-compose-client-server.yml`.
- Port map: client `5175`, API `5241`, Postgres host `5444`.
- Vite alias `@` maps to `src`; dev proxy forwards `/api` to local backend `http://localhost:5242` (`Source/Client/pot-react/vite.config.ts`).

## Server Test Execution Reliability

- For targeted server integration tests, prefer project-level .NET execution from `Source/Server`:
  - `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`
- Prefer this over file/path-based .NET test discovery for deterministic targeted runs.

## Key References

- Architecture: `Docs/ARCHITECTURE.md`
- Local setup: `Docs/LOCAL-SETUP.md`
- Docker setup: `Docs/DOCKER-SETUP.md`, `Source/Docker/DEVELOPER.md`
