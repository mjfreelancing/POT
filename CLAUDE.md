# POT

POT is projection-first: optimize for future cash-flow projections, not historical budgeting.

Layout: .NET server in `Source/Server` (`pot.sln`), React/Vite client in `Source/Client/pot-react`, Docker assets in `Source/Docker`. Area-specific rules live in nested `CLAUDE.md` files (loaded when you work in that directory) and in `.claude/rules/`.

## Working agreement

- Stay within the requested scope; do not make unrelated changes. If scope is unclear, ask or recommend before touching anything outside the request.
- When troubleshooting, present options and ask the user to choose; do not pick one automatically.
- Search for existing code to reuse before writing new code.
- Before creating or moving code into shared helpers/utils or a shared project/package (production or test), ask the user where it should go.
- Preserve public API shape unless a change is explicitly requested or approved.
- Never reference PRDs, ADRs, task lists, stage numbers or requirement IDs (e.g. `PRD-025`, `R6`, `A-08`, `Stage 2`) in code, tests, comments or test names. Describe the behaviour itself; those documents are temporary and the references go stale.
- Never remove existing comments. If one looks out of date, ask before changing it (spelling fixes are fine).
- Never generate EF Core migrations unless explicitly asked; after editing entities or schema-impacting models, tell the developer to run and review `add-migration`.
- Never touch `Source/Docker/postgres-data/**`.
- Never run a git command that changes the repository or working tree (including `git mv`, `git rm`, `git add`, `git stash`, `git checkout`, `git restore`, `git reset`, `git commit` and `git push`) without the user's explicit approval. Read-only commands (`git status`, `git diff`, `git log`, `git show`, `git blame`) are fine. The user reviews code changes and stages/commits themselves; make edits in the working tree only. Rename or delete files with ordinary file operations rather than their git equivalents, and ask first if a git write is genuinely needed.

## Code style (all languages)

- Clear names; no one-letter variables, including lambda parameters (`order => order.CalculateTotal()`, not `x => ...`). Use `item` when the element meaning is not obvious.
- In multi-line boolean expressions, put `&&` / `||` at the end of the preceding line.
- Do not construct model/request objects inline in call arguments; assign to a named variable first.
- Keep code units focused; avoid unnecessary abstraction, cyclic dependencies and hidden side effects.
- Keep concerns separated (transport, orchestration, domain/application logic, persistence, UI state).
- Handle success/failure outcomes explicitly. Keep boundary contracts stable.
- When a language-specific rule (nested `CLAUDE.md` or `.claude/rules/`) conflicts with this section, the specific rule wins.

## Running things

| Task | Command / entry point |
| --- | --- |
| Full stack up / down | VS Code tasks `docker-start-client-server` / `docker-stop-client-server` |
| Compose file | `Source/Docker/docker-compose-client-server.yml` |
| Server (from `Source/Server`) | `dotnet run --project Pot.Data.Migrations`, `dotnet run --project Pot.AspNetCore` |
| Server tests (all) | `dotnet test pot.sln -c Debug --nologo --verbosity minimal` from `Source/Server` |
| Targeted server tests | `dotnet test <Project>/<Project>.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"` from `Source/Server` (prefer this over file-path-based test discovery) |
| Server coverage | `Source/Server/code_coverage.ps1` or task `server-run-test-coverage` |
| Client tests | `npm run test` (`npm run test:ui` for UI mode) from `Source/Client/pot-react` |

Ports: client `5175`, API `5241` (Docker), Postgres host `5444`. Non-Docker backend used by the Vite dev proxy is `5242` (`/api` is proxied there; alias `@` maps to `src`). Health: API `/_health`, client `/health`.

## References

- Architecture: `Docs/ARCHITECTURE.md`
- Local setup: `Docs/LOCAL-SETUP.md`
- Docker setup: `Docs/DOCKER-SETUP.md`, `Source/Docker/DEVELOPER.md`
- PRDs / ADRs: `Docs/Future/`
