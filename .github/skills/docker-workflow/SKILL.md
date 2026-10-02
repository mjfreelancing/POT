---
name: docker-workflow
description: Build, start, stop, check status of, or health-check the POT Docker stack (client, API, Postgres) using the repository tasks and compose file.
disable-model-invocation: true
---

# Docker Workflow

Use the safest, most repeatable path. Rules for this area are in `.github/instructions/docker.instructions.md`.

1. Prefer the VS Code tasks `docker-start-client-server` / `docker-stop-client-server` over ad-hoc shell sequences.
2. When running compose directly, use `Source/Docker/docker-compose-client-server.yml` with `--env-file .env --env-file .env.development`.
3. Never modify `Source/Docker/postgres-data/**` or any volume unless the user explicitly asks.
4. Validate afterwards:
   - containers `pot-react`, `pot-aspnet` and `pot-postgres` are running;
   - `http://localhost:5241/_health` (API) and `http://localhost:5175/health` (client) respond.

If a step fails, summarize the root error and give the smallest next corrective step.
