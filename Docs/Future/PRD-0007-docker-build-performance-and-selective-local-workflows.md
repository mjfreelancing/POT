# Docker Build Performance and Selective Local Workflows PRD

Status: Planned
Priority: High
Last Updated: 2026-04-08

## Problem Statement

Current Docker workflows are slower than needed for day-to-day development and deployment planning:

- Server image build currently uses solution-level `dotnet restore` and `dotnet build`, which traverses test projects that are not needed for runtime image output.
- Local and Azure VS Code tasks enforce `--no-cache`, which disables Docker layer reuse and increases build time.
- Local Docker workflows are full-stack focused, while developers sometimes only need server or only client.

The result is longer feedback cycles for local iteration and repeated deployment overhead for Azure pushes.

## Goals

- Reduce local and Azure Docker build duration without changing runtime behavior.
- Exclude test projects from server container build graph used for runtime artifacts.
- Keep existing full-stack local workflow intact.
- Add explicit server-only and client-only local Docker workflows in parallel with existing full-stack tasks.
- Keep task usage explicit and repeatable through VS Code tasks and documented commands.

## Non-Goals

- No changes to application business logic or API contracts.
- No replacement of current Docker compose architecture.
- No removal of existing full-stack tasks.
- No reduction in automated test coverage strategy outside image build path.

## Current State Summary

1. Server Docker build path:

- Dockerfile copies all `Server` sources, then runs solution-level `dotnet restore` and `dotnet build` before publish.
- The solution includes multiple test projects (`Pot.App.Tests`, `Pot.Data.Tests`, `Pot.AspNetCore.Tests`, `Pot.AspNetCore.Integration.Tests`, `Pot.Shared.Tests`) and `Pot.TestUtils`.
- Runtime image only needs published outputs for `Pot.AspNetCore` and `Pot.Data.Migrations`.

2. Local VS Code full-stack task:

- `docker-start-pot-client-server` performs `docker-compose ... build --no-cache` and then `up -d`.
- This disables reuse of cached build layers for both server and client.

3. Azure VS Code tasks:

- `azure-pot-server-build-and-deploy` uses `docker build --no-cache`.
- `azure-pot-client-build-and-deploy` uses `docker build --no-cache` with Azure build args.

4. Local selective workflows:

- Compose file defines separate `server` and `client` services, but task set does not provide first-class server-only or client-only variants.

## Product Decisions (Proposed)

1. Build cache policy:

- Default tasks should use cache-enabled builds.
- Plan to add explicit `-no-cache` task variants for troubleshooting and deterministic rebuild needs.

2. Server runtime build graph policy:

- Server Dockerfile should restore/publish runtime projects directly (`Pot.AspNetCore`, `Pot.Data.Migrations`) instead of solution-level restore/build.
- Test projects remain in repository and CI test workflows, but are not part of runtime image build graph.

3. Local workflow policy:

- Preserve existing full-stack start/stop tasks.
- Add server-only and client-only task flows for build/start/stop where practical.
- Client-only local flow assumes backend is already available and running.

4. Azure deployment policy:

- Keep existing Azure tasks and image tags.
- Switch default Azure build tasks to cached mode.
- Add `-no-cache` Azure task variants for forced rebuilds.

5. Task naming policy:

- Use the descriptive existing naming pattern with the `pot-` segment removed from labels.
- Rename existing full-stack start/stop task labels to match the new naming convention.
- Confirmed task labels:
  - `docker-start-client-server`
  - `docker-stop-client-server`
  - `docker-start-client-server-no-cache`
  - `docker-start-server`
  - `docker-stop-server`
  - `docker-start-server-no-cache`
  - `docker-start-client`
  - `docker-stop-client`
  - `docker-start-client-no-cache`
  - `azure-server-build-and-deploy`
  - `azure-server-build-and-deploy-no-cache`
  - `azure-client-build-and-deploy`
  - `azure-client-build-and-deploy-no-cache`

6. Advanced Docker cache policy:

- Defer `buildx` cache strategy to a later iteration after baseline improvements are complete and validated.

## Proposed Scope

### A) Server Dockerfile Optimization

1. Replace solution-level restore/build path with runtime-project-targeted operations:

- Restore `Pot.AspNetCore/Pot.AspNetCore.csproj`.
- Restore `Pot.Data.Migrations/Pot.Data.Migrations.csproj`.
- Publish both projects in Release mode.
- Project-targeted restore/publish still traverses and builds transitive project references required by those runtime projects, while avoiding unrelated solution projects (for example test projects).

2. Remove solution-wide `dotnet build` layer from Docker image build path where redundant.

3. Keep runtime stage behavior unchanged (entrypoint, migrations-first startup, non-root user, exposed port).

### B) VS Code Task Enhancements

1. Existing full-stack tasks:

- Keep full-stack start/stop tasks with renamed labels: `docker-start-client-server` and `docker-stop-client-server`.
- Remove `--no-cache` from default full-stack build task.
- Add explicit `-no-cache` variant: `docker-start-client-server-no-cache`.

2. New local selective tasks:

- Server-only:
  - Start/build server + postgres.
  - Stop server + postgres.
- Client-only:
  - Start/build client only.
  - Stop client only.

3. Azure tasks:

- Use renamed default labels: `azure-server-build-and-deploy` and `azure-client-build-and-deploy`.
- Remove `--no-cache` in default variants.
- Add `-no-cache` variants using `--no-cache`: `azure-server-build-and-deploy-no-cache` and `azure-client-build-and-deploy-no-cache`.

### C) Documentation Updates

1. Update Docker developer docs with:

- Cache-enabled default task policy.
- `-no-cache` troubleshooting commands.
- Server-only/client-only local workflow commands.

2. Update Azure deployment notes with:

- Cache-enabled default build commands.
- Optional `-no-cache` build commands and when to use them.

## Option Analysis

1. Option A (Recommended): Runtime-project-targeted restore/publish in Dockerfile

- Pros:
  - Minimal structural change.
  - Directly removes test project traversal from image build graph.
  - Fast to implement and verify.
- Cons:
  - Requires careful Dockerfile layer ordering to preserve cache benefit.

2. Option B: Runtime-only `.slnf` (solution filter) for Docker builds

- Pros:
  - Clear declaration of runtime project set.
  - Useful for broader developer workflows.
- Cons:
  - Additional artifact to maintain.
  - Slightly higher ongoing maintenance overhead.

Decision (Confirmed): Option A is selected for this iteration. Option B is deferred and can be revisited if runtime build graph management expands.

## Acceptance Criteria

1. Server Docker image build does not perform solution-level restore/build of test projects.
2. Existing full-stack local task remains available and functionally equivalent from a user perspective.
3. Default local full-stack task uses Docker cache by default.
4. Default Azure server/client tasks use Docker cache by default.
5. Explicit `-no-cache` variants exist for local full-stack and Azure server/client tasks.
6. Server-only local task flow exists and can run without building client image.
7. Client-only local task flow exists and can run without building server image.
8. Docker documentation includes updated task matrix and usage guidance.

## Validation Plan

1. Baseline timing capture (before changes):

- Full-stack local build + up.
- Azure server build command.
- Azure client build command.

2. Post-change timing capture:

- Same commands/workflows with cached defaults.
- `-no-cache` variants measured separately.

3. Functional validation:

- `/_health` for API and `/health` for frontend remain healthy in local full-stack flow.
- Server-only and client-only tasks perform expected targeted lifecycle behavior.

## Risks and Mitigations

1. Risk: Cache can hide stale dependency issues.

- Mitigation: Provide explicit `-no-cache` task variants and document usage triggers.

2. Risk: Project-targeted restore/publish might miss an implicit dependency previously surfaced by solution build.

- Mitigation: Validate full publish output and startup smoke tests before rollout completion.

3. Risk: Increased task count may reduce discoverability.

- Mitigation: Keep naming explicit and add concise task matrix in docs.

## Rollout Plan

1. Phase 1 (Task policy):

- Introduce cache-enabled defaults and `-no-cache` variants for local full-stack and Azure tasks.

2. Phase 2 (Server Dockerfile optimization):

- Move to runtime-project-targeted restore/publish.

3. Phase 3 (Selective local workflows):

- Add server-only and client-only task flows.

4. Phase 4 (Docs and timing report):

- Publish updated developer guidance and before/after timing summary.

## Effort Estimate

- Task updates: 0.5 day.
- Server Dockerfile optimization: 0.5-1 day.
- Documentation updates: 0.25 day.
- Optional advanced build caching (`buildx` registry cache): additional 0.5 day.

## Open Questions

None.

## Supplementary Implementation Note (2026-05-03)

After production deployment validation, the cache-enabled task variants proposed in this PRD were removed from `.vscode/tasks.json`.

What changed:

- Removed duplicate `-no-cache` task labels.
- Kept canonical task labels (for example `docker-start-client-server`, `azure-server-build-and-deploy`) and made them always run with `--no-cache`.

Why this change was made:

- A cache-enabled deploy task completed in seconds with all build/publish layers marked `CACHED`, which made it possible to push an image that did not include the latest code changes.
- For deployment workflows, deterministic freshness is preferred over build speed.
- Removing parallel cache/no-cache task variants reduces operator error risk (running the wrong similarly named task).

Resulting policy:

- Docker task naming is now singular per workflow (no cache suffix variants).
- VS Code Docker build/deploy tasks prioritize correctness and release safety by always forcing rebuilds.

## Implementation Validation Snapshot (2026-05-01)

This section captures the execution outcomes from the completed implementation run so timing evidence is retained independently of temporary execution checklists.

### Timing Comparison (Baseline vs Post-Change)

| Workflow                    | Baseline | Post-Change (default cached path) | Delta                       |
| --------------------------- | -------- | --------------------------------- | --------------------------- |
| Full-stack local build + up | `99.94s` | `13.62s`                          | `-86.32s` (~`86.4%` faster) |
| Azure server build-only     | `58.73s` | `6.22s`                           | `-52.51s` (~`89.4%` faster) |
| Azure client build-only     | `43.99s` | `33.49s`                          | `-10.50s` (~`23.9%` faster) |

Notes:

- Azure measurements are build-only (push excluded) for deterministic local timing comparison.
- Full-stack measurement includes `build` and `up -d` path used by the default local task.

### Functional Validation Snapshot

- Full-stack API health `http://localhost:5241/_health`: `200`
- Full-stack frontend health `http://localhost:5175/health`: `200`
- Server-only API health `http://localhost:5241/_health`: `200`
- Client-only frontend health `http://localhost:5175/health`: `200`

### Acceptance Criteria Cross-Check

- PRD acceptance criteria 1-8 were validated as satisfied during final execution checks.
- No regressions were identified in runtime health behavior after the Docker/task changes.
