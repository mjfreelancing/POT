# AI Instructions Notes

This folder contains Copilot instruction routing for this repo.

## Current Setup

- Required VS Code setting: `.vscode/settings.json` with `github.copilot.chat.codeGeneration.useInstructionFiles: true`

- Global router: `.github/copilot-instructions.md`
- C# scoped rules: `.github/instructions/csharp.instructions.md` (`applyTo: Source/Server/**/*.cs`)
- Client scoped rules: `.github/instructions/client.instructions.md` (`applyTo: Source/Client/pot-react/**`)
- TypeScript scoped rules: `.github/instructions/typescript.instructions.md` (`applyTo: Source/Client/pot-react/src/**/*.{ts,tsx}`)
- Client test scoped rules: `.github/instructions/client.tests.instructions.md` (`applyTo: Source/Client/pot-react/tests/**`)
- Server scoped rules: `.github/instructions/server.instructions.md` (`applyTo: Source/Server/**`)
- Server test scoped rules: `.github/instructions/server.tests.instructions.md` (`applyTo: Source/Server/*Tests/**`)
- Integration test scoped rules: `.github/instructions/integration-tests.instructions.md` (`applyTo: Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs`)
- Docker scoped rules: `.github/instructions/docker.instructions.md` (`applyTo: Source/Docker/**`)

## How Scope Is Chosen

- **Automatic**: scope is selected from `applyTo` based on the file path being edited.
  - Editing `Source/Server/**/*.cs` => C# rules apply.
  - Editing `Source/Client/pot-react/...` => client rules apply.
  - Editing `Source/Client/pot-react/src/**/*.ts(x)` => TypeScript rules apply.
  - Editing `Source/Client/pot-react/tests/...` => client test rules apply.
  - Editing `Source/Server/...` => server rules apply.
  - Editing `Source/Server/*Tests/...` => server test rules apply.
  - Editing `Source/Server/Pot.AspNetCore.Integration.Tests/**/*.cs` => integration test rules apply.
  - Editing `Source/Docker/...` => docker rules apply.

## How To Force Scope In Chat

Use one of these in your prompt:

- `Use client instructions only for this task.`
- `Use C# instructions only for this task.`
- `Use TypeScript instructions only for this task.`
- `Use client test instructions only for this task.`
- `Use server instructions only for this task.`
- `Use server test instructions only for this task.`
- `Use docker instructions only for this task.`
- `Follow .github/instructions/client.instructions.md for this change.`
- `Working only under Source/Server for this task.`

## Notes

- Keep `.github/copilot-instructions.md` short and shared (project-wide context + workflow links).
- Keep implementation details in path-scoped instruction files to reduce noise.
- Keep instructions concrete and aligned to current architecture and workflows.
- Folder indexes (standardized as Purpose -> Contents -> Source of truth boundary -> Notes):
  - Instructions: `.github/instructions/README.md`
  - Prompts: `.github/prompts/README.md`
  - Scripts: `.github/scripts/README.md`
  - Skills: `.github/skills/README.md`
- Integration tests live under `Source/Server/Pot.AspNetCore.Integration.Tests` and should use the integration-test instruction/prompt guidance.
- Reusable prompts currently available under `.github/prompts`: `repo_tests.prompt.md`, `server_unit_test.prompt.md`, `server_coverage.prompt.md`, `client_tests.prompt.md`, `document_csharp.prompt.md`, `document_typescript.prompt.md`, `server_integration_test.prompt.md`, `docker_workflow.prompt.md`, `feature_implementation.prompt.md`.
- Reusable skills available under `.github/skills`: see `.github/skills/README.md` (includes `environment-preflight`).
- Automation guidance: `.github/automation-playbook.md`
- Copilot agent hooks: `.github/hooks/hooks.json` with scripts under `.github/hooks/scripts/`.
