# GitHub Copilot configuration (VS Code)

Instructions for GitHub Copilot in this repository.

## Where guidance lives

| Location | Loaded | Contents |
| --- | --- | --- |
| `.github/copilot-instructions.md` | Always | Working agreement, cross-language style, run commands, ports |
| `.github/instructions/*.instructions.md` | When a file matching `applyTo` is in context | Area and language rules (table below) |
| `.github/skills/<name>/SKILL.md` | On demand, or via `/<name>` | Workflows (tests, coverage, Docker, PRDs, docs) |
| `.github/scripts/` | Run manually | `agent-env-tools` environment diagnostics |

## Scoped instructions

| File | `applyTo` |
| --- | --- |
| `server.instructions.md` | `Source/Server/**` |
| `allsoverit-patterns.instructions.md` | `Source/Server/**/*.cs` |
| `dotnet-tests.instructions.md` | `Source/Server/*Tests/**/*.cs` |
| `aspnetcore-integration-tests.instructions.md` | `Source/Server/Pot.AspNetCore.Integration.Tests/**` |
| `client.instructions.md` | `Source/Client/pot-react/**` |
| `client-tests.instructions.md` | `Source/Client/pot-react/tests/**` |
| `e2e.instructions.md` | `Source/Client/pot-react/e2e/**` |
| `docker.instructions.md` | `Source/Docker/**` |

## Required setting

`.vscode/settings.json` sets `github.copilot.chat.codeGeneration.useInstructionFiles: true`; without it Copilot may not reliably load the scoped instruction files.

## Guardrails

Instruction text is advisory. `.vscode/settings.json` enforces the important guardrails: it sets `chat.tools.edits.autoApprove` so edits to protected paths (`Source/Docker/postgres-data/**`, `Source/Client/pot-react/src/components/ui/**`, `.env*`) always require manual approval, and `chat.tools.terminal.autoApprove` lists the commands that run without prompting.

## Adding things

- **A rule for one area or glob**: add `.github/instructions/<topic>.instructions.md` with `description` and `applyTo` front matter. Avoid `applyTo: "**/*"`; always-on guidance belongs in `copilot-instructions.md`.
- **A multi-step workflow**: add a skill (see `skills/README.md`).
- Keep each file concise and state rules concretely.
