# AI Instructions Notes

This folder contains Copilot instruction routing for this repo.

## Structure Model

- `copilot-ai-pack` stores reusable assets under `instructions`, `prompts`, and `scripts/<pack>/...`.
  - Source repository: `https://github.com/mjfreelancing/copilot-ai-pack`
- In a destination repo like POT, those assets land under this `.github/` folder by type (`instructions`, `prompts`, `scripts`).
- Future baseline updates should compare AI-pack source files by type and sync them into this repo's `.github/**` layout.

## Current Setup

- Required VS Code setting: `.vscode/settings.json` with `github.copilot.chat.codeGeneration.useInstructionFiles: true`

- Global router: `.github/copilot-instructions.md`
- Language-agnostic baseline: `.github/instructions/language-agnostic-core.instructions.md` (`applyTo: **/*`)
- ASP.NET Core API baseline: `.github/instructions/aspnetcore-api.instructions.md` (`applyTo: **/*.cs`)
- C# scoped rules: `.github/instructions/csharp.instructions.md` (`applyTo: Source/Server/**/*.cs`)
- .NET test baseline: `.github/instructions/dotnet.tests.instructions.md` (`applyTo: Source/Server/*Tests/**/*.cs`)
- React client scoped rules: `.github/instructions/react-client.instructions.md` (`applyTo: Source/Client/pot-react/**`)
- TypeScript scoped rules: `.github/instructions/typescript.instructions.md` (`applyTo: Source/Client/pot-react/src/**/*.{ts,tsx}`)
- Client test scoped rules: `.github/instructions/client.tests.instructions.md` (`applyTo: Source/Client/pot-react/tests/**`)
- Postgres/EF Core baseline: `.github/instructions/postgres-efcore.instructions.md` (`applyTo: **/*.cs`)
- Server scoped rules: `.github/instructions/server.instructions.md` (`applyTo: Source/Server/**`)
- Integration test scoped rules: `.github/instructions/aspnetcore.integration-tests.instructions.md` (`applyTo: Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs`)
- Docker scoped rules: `.github/instructions/docker.instructions.md` (`applyTo: Source/Docker/**`)

## How Scope Is Chosen

- **Automatic**: scope is selected from `applyTo` based on the file path being edited.
  - Editing `Source/Server/**/*.cs` => C# rules apply.
  - Editing `Source/Client/pot-react/...` => React client rules apply.
  - Editing `Source/Client/pot-react/src/**/*.ts(x)` => TypeScript rules apply.
  - Editing `Source/Client/pot-react/tests/...` => client test rules apply.
  - Editing `Source/Server/...` => server rules apply.
  - Editing `Source/Server/*Tests/...` => .NET test rules apply.
  - Editing `Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs` => ASP.NET Core integration test rules apply.
  - Editing `Source/Docker/...` => docker rules apply.

## How To Force Scope In Chat

Use one of these in your prompt:

- `Use client instructions only for this task.`
- `Use C# instructions only for this task.`
- `Use TypeScript instructions only for this task.`
- `Use client test instructions only for this task.`
- `Use server instructions only for this task.`
- `Use .NET test instructions only for this task.`
- `Use docker instructions only for this task.`
- `Follow .github/instructions/react-client.instructions.md for this change.`
- `Working only under Source/Server for this task.`

## Notes

- Keep `.github/copilot-instructions.md` short and shared (project-wide context + workflow links).
- Keep implementation details in path-scoped instruction files to reduce noise.
- Keep instructions concrete and aligned to current architecture and workflows.
- Integration tests live under `Source/Server/Pot.AspNetCore.Integration.Tests` and should use the integration-test instruction/prompt guidance.
- Reusable prompts currently available under `.github/prompts`: `repo_tests.prompt.md`, `dotnet_unit_test.prompt.md`, `dotnet_integration_test.prompt.md`, `typescript_tests.prompt.md`, `client_tests.prompt.md`, `server_unit_test.prompt.md`, `server_integration_test.prompt.md`, `code_coverage.prompt.md`, `document_csharp.prompt.md`, `document_typescript.prompt.md`, `docker_workflow.prompt.md`, `feature_implementation.prompt.md`.
- Reusable scripts available under `.github/scripts`: see `.github/scripts/agent-env-tools/README.md` for agent-env-tools usage.
- Per-change cleanup automation is save-action based via `.vscode/settings.json` (`editor.formatOnSave` + `source.organizeImports`).
