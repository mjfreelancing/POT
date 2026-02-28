---
name: docker_workflow
description: Run POT docker lifecycle workflows (build/start/stop/health) with task-first defaults.
---

Handle Docker workflows for POT using the safest, repeatable path.

## Scope

- Handle build/start/stop/health Docker workflows for the existing POT stack.

## Workflow

- Prefer task/command wrappers over ad-hoc shell sequences.
- Validate health/status after lifecycle changes.

## Commands and Rules

- Prefer VS Code tasks first:
  - Start stack: `docker-start-pot-client-server`
  - Stop stack: `docker-stop-pot-client-server`
- Use compose file `Source/Docker/docker-compose-client-server.yml` when direct compose commands are required.
- Include env files when using compose directly:
  - `--env-file .env --env-file .env.development`
- Keep data safety boundaries:
  - never modify or delete `Source/Docker/postgres-data/**` in normal operations.

## Validation

- Confirm containers are running (`pot-react`, `pot-aspnet`, `pot-postgres`).
- Confirm API health endpoint responds at `http://localhost:5241/_health`.
- Confirm frontend health endpoint responds at `http://localhost:5175/health`.

If a step fails, summarize root error and provide the smallest next corrective step.

## Expansion Notes

- Add new lifecycle commands under `Commands and Rules` with exact task names or compose paths.
- Keep service-specific safety boundaries in this prompt aligned with `docker.instructions.md`.
