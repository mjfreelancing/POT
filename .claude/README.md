# Claude Code configuration

Instructions for Claude Code in this repository. (`.github/` holds the separate GitHub Copilot equivalents; they are not read by Claude Code and are not kept in sync with these files.)

## Where guidance lives

| Location | Loaded | Contents |
| --- | --- | --- |
| `CLAUDE.md` (repo root) | Always | Working agreement, cross-language style, run commands, ports |
| `Source/Server/CLAUDE.md` | When working in `Source/Server` | C#/.NET, ASP.NET Core, EF Core, DI, XML docs |
| `Source/Server/Pot.AspNetCore.Integration.Tests/CLAUDE.md` | When working in that project | Hosted API integration tests |
| `Source/Client/pot-react/CLAUDE.md` | When working in the client | React and TypeScript |
| `Source/Client/pot-react/tests/CLAUDE.md` | When working in client tests | Vitest conventions |
| `Source/Client/pot-react/e2e/CLAUDE.md` | When working in e2e | Playwright (imports the e2e README) |
| `Source/Docker/CLAUDE.md` | When working in Docker | Compose, ports, health, protected data |
| `.claude/rules/*.md` | When a file matching `paths:` is read | `dotnet-tests`, `allsoverit-patterns` |
| `.claude/skills/<name>/SKILL.md` | On demand, or via `/<name>` | Workflows (tests, coverage, Docker, PRDs, docs) |
| `.claude/scripts/` | Run manually | `agent-env-tools` environment diagnostics |

Use `/memory` to see which instruction files are loaded in a session.

## Adding things

- **A rule for one directory**: put a `CLAUDE.md` in that directory.
- **A rule for a glob spanning directories**: add `.claude/rules/<topic>.md` with `paths:` frontmatter (`applyTo` is a Copilot field and is ignored).
- **A multi-step workflow**: add `.claude/skills/<name>/SKILL.md` with `name` and a specific `description`. Set `disable-model-invocation: true` if it has side effects.
- **Something that must always hold**: use `.claude/settings.json` permissions or a hook; `CLAUDE.md` text is advisory.
- Keep each `CLAUDE.md` under about 200 lines and state rules concretely. Put personal overrides in `CLAUDE.local.md` (gitignored).
