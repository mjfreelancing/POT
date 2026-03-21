# POT Copilot Instructions (Global)

POT is projection-first: optimize for future cash-flow projections, not historical budgeting.

Instruction authoring model:

- Keep standards concrete and repository-specific.
- Keep prompts workflow-focused and avoid duplicating instruction rules.
- Keep implementation details in path-scoped instruction files.

Use path-scoped instructions for implementation details:

- Language-agnostic baseline: `.github/instructions/language-agnostic-core.instructions.md` (`applyTo: **/*`)
- ASP.NET Core API baseline: `.github/instructions/aspnetcore-api.instructions.md` (`applyTo: **/*.cs`)
- C# scope: `.github/instructions/csharp.instructions.md` (`applyTo: Source/Server/**/*.cs`)
- .NET test baseline: `.github/instructions/dotnet.tests.instructions.md` (`applyTo: Source/Server/*Tests/**/*.cs`)
- React client scope: `.github/instructions/react-client.instructions.md` (`applyTo: Source/Client/pot-react/**`)
- TypeScript scope: `.github/instructions/typescript.instructions.md` (`applyTo: Source/Client/pot-react/src/**/*.{ts,tsx}`)
- Client tests scope: `.github/instructions/client.tests.instructions.md` (`applyTo: Source/Client/pot-react/tests/**`)
- Postgres/EF Core baseline: `.github/instructions/postgres-efcore.instructions.md` (`applyTo: **/*.cs`)
- Server scope: `.github/instructions/server.instructions.md` (`applyTo: Source/Server/**`)
- Integration tests scope: `.github/instructions/aspnetcore.integration-tests.instructions.md` (`applyTo: Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs`)
- Docker scope: `.github/instructions/docker.instructions.md` (`applyTo: Source/Docker/**`)

## Shared Integration Workflows

- Preferred full-stack startup in VS Code: task `docker-start-pot-client-server`; stop with `docker-stop-pot-client-server`.
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
