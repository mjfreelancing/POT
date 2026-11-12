# POT

POT is projection-first: optimize for future cash-flow projections, not historical budgeting.

Layout: .NET server in `Source/Server` (`pot.sln`), React/Vite client in `Source/Client/pot-react`, Docker assets in `Source/Docker`. Area-specific rules live in scoped files in `.github/instructions/` (applied by `applyTo` glob) and multi-step workflows in `.github/skills/`.

## Working agreement

- Stay within the requested scope; do not make unrelated changes. If scope is unclear, ask or recommend before touching anything outside the request.
- When troubleshooting, present options and ask the user to choose; do not pick one automatically.
- Search for existing code to reuse before writing new code.
- Before creating or moving code into shared helpers/utils or a shared project/package (production or test), ask the user where it should go when the decision is not obvious.
- Preserve public API shape unless a change is explicitly requested or approved.
- Never reference PRDs, ADRs, task lists, stage numbers or requirement IDs (e.g. `PRD-0025`, `R6`, `A-08`, `Stage 2`) in code, tests, comments or test names. Describe the behaviour itself. Task lists are temporary; PRDs and ADRs are permanent but are cited from docs and instruction files only — code states the rule itself.
- Before an architectural change, read `Docs/adr/README.md`. Do not contradict an accepted ADR without asking. A change that makes a new architectural decision adds a proposed ADR in the same change.
- Never remove existing comments. If one looks out of date, ask before changing it (spelling fixes are fine).
- Never generate EF Core migrations unless explicitly asked; after editing entities or schema-impacting models, ask the developer to add a migration and review it — `Running things` has the command.
- Never touch `Source/Docker/postgres-data/**`.
- Never run a git command that changes the repository or working tree (including `git mv`, `git rm`, `git add`, `git stash`, `git checkout`, `git restore`, `git reset`, `git commit` and `git push`) without the user's explicit approval. Read-only commands (`git status`, `git diff`, `git log`, `git show`, `git blame`) are fine. The user reviews code changes and stages/commits themselves; make edits in the working tree only. Rename or delete files with ordinary file operations rather than their git equivalents, and ask first if a git write is genuinely needed.

## Code style (all languages)

- Clear names; no one-letter variables, including lambda parameters (`order => order.CalculateTotal()`, not `x => ...`). Use `item` when the element meaning is not obvious.
- In multi-line boolean expressions, put `&&` / `||` at the end of the preceding line.
- Do not construct model/request objects inline in call arguments; assign to a named variable first.
- Keep code units focused; avoid unnecessary abstraction, cyclic dependencies and hidden side effects.
- Keep concerns separated (transport, orchestration, domain/application logic, persistence, UI state).
- Handle success/failure outcomes explicitly. Keep boundary contracts stable.
- When a language-specific rule (a scoped file in `.github/instructions/`) conflicts with this section, the specific rule wins.

## Running things

| Task                          | Command / entry point                                                                                                                                                                                   |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Full stack up / down          | VS Code tasks `docker-start-client-server` / `docker-stop-client-server`                                                                                                                                |
| Compose file                  | `Source/Docker/docker-compose-client-server.yml`                                                                                                                                                        |
| Server (from `Source/Server`) | `dotnet run --project Pot.Data.Migrations`, `dotnet run --project Pot.AspNetCore`                                                                                                                       |
| Server tests (all)            | `dotnet test pot.sln -c Debug --nologo --verbosity minimal` from `Source/Server`                                                                                                                        |
| Targeted server tests         | `dotnet test <Project>/<Project>.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"` from `Source/Server` (prefer this over file-path-based test discovery)                                       |
| Server coverage               | `Source/Server/code_coverage.ps1` or task `server-run-test-coverage`                                                                                                                                    |
| EF Core migration             | `dotnet ef migrations add <Name> --project Pot.Data --startup-project Pot.Data.Migrations` from `Source/Server` — only with a developer-approved name, never one you invent (see the working agreement) |
| Client tests                  | `npm run test` (`npm run test:ui` for UI mode) from `Source/Client/pot-react`                                                                                                                           |

Ports: client `5175`, API `5241` (Docker), Postgres host `5444`. Non-Docker backend used by the Vite dev proxy is `5242` (`/api` is proxied there; alias `@` maps to `src`). Health: API `/_health`, client `/health`.

## References

- Docs map and where each kind of document goes: `Docs/README.md`
- Architecture: `Docs/ARCHITECTURE.md`
- Decisions (ADRs): `Docs/adr/README.md`
- Coding conventions: `Docs/standards/`
- Local setup: `Docs/how-to/run-locally-without-docker.md`
- Docker setup: `Docs/how-to/run-the-docker-stack.md`, `Source/Docker/DEVELOPER.md`
- PRDs: `Docs/prd/`
