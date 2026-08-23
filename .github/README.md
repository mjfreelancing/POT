# POT Copilot Assets

This folder holds the Copilot assets for the POT repository: instruction files, prompts, skills, and environment scripts that shape how Copilot works in this repo.

POT is projection-first: optimize for future cash-flow projections, not historical budgeting.

## Entry point

- `.github/copilot-instructions.md` — repository-level operating guide for Copilot (always loaded). Read it first; it lists every scoped instruction and the shared integration workflows (tasks, ports, proxy).

## Required setting

- `.vscode/settings.json` sets `github.copilot.chat.codeGeneration.useInstructionFiles: true`; without it Copilot may not reliably load the scoped instruction files.

## Folder contents

| Folder          | Purpose                                                                                                               |
| --------------- | --------------------------------------------------------------------------------------------------------------------- |
| `instructions/` | Always-on, path-scoped guidance loaded per file pattern. See `instructions/README.md` for the catalog.                |
| `prompts/`      | Reusable task entry points for common testing/documentation work. See `prompts/README.md`.                            |
| `skills/`       | On-demand multi-step workflows (slash commands) such as coverage, Docker lifecycle, and PRDs. See `skills/README.md`. |
| `scripts/`      | Copyable utility packs, currently `agent-env-tools` (environment diagnostics). See `scripts/README.md`.               |

## Provenance and mirror workflow

These assets are adapted from [`copilot-ai-pack`](https://github.com/mjfreelancing/copilot-ai-pack), the source of truth for the reusable templates. They follow the mirror convention:

- `Core Rules` and baseline prompt/skill content are template-owned. Keep them text-equivalent to the pack unless a baseline update is intentionally deferred.
- POT-specific behavior lives only in `Expansion Notes` / `Repository Notes` (for example route anchors, selector anchors, project names, commands, and conventions).
- When the pack changes, sync mirrors by copying updated baseline content and preserving only POT-specific additions.
- `applyTo` front matter is template-owned; adapt the scope per consuming repo (already done for POT paths).

## POT-specific highlights

- Preferred full-stack startup: VS Code task `docker-start-client-server`; stop with `docker-stop-client-server`.
- Docker compose reference: `Source/Docker/docker-compose-client-server.yml`.
- Port map: client `5175`, API `5241`, Postgres host `5444`; dev proxy forwards `/api` to `http://localhost:5242`.
- Server test execution from `Source/Server`; targeted integration runs use `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`.

## Maintainer notes

When changing assets:

- keep each asset intent clear and scoped
- update the relevant folder `README.md` when files or guidance change
- keep this README aligned with the actual folder structure
- keep POT-only content in `Expansion Notes` / `Repository Notes` so future baseline syncs stay clean
