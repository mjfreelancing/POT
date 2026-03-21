---
applyTo: "Source/Docker/**"
---

# Docker Workflow Instructions

## Core Rules

### Core Workflow

- Prefer VS Code tasks for full-stack lifecycle:
  - Start: `docker-start-pot-client-server`
  - Stop: `docker-stop-pot-client-server`
- Primary compose file is `Source/Docker/docker-compose-client-server.yml`.
- Compose commands should load both env files when used directly:
  - `--env-file .env --env-file .env.development`

## Ports and Services

- Expected host ports: client `5175`, API `5241`, Postgres `5444`.
- Expected containers: `pot-react` (client), `pot-aspnet` (server), `pot-postgres` (database).
- API health endpoint: `http://localhost:5241/_health`.
- Frontend health endpoint: `http://localhost:5175/health`.

## Build and Config Conventions

- Keep server and client images aligned with existing naming/tagging patterns (`pot-server`, `pot-client`, timestamp + `latest`).
- Client runtime config is build-time in Docker (`Docker/Client/Dockerfile` args); changing API base URL requires rebuild.
- Keep environment variable naming consistent with existing compose/appsettings mapping (for example `DATABASE__*`, `JWT__*`, `SMTP__*`, `CORS__*`).

### Safety

- Treat `Source/Docker/postgres-data/**` as runtime data; do not modify/delete in normal code changes.
- Keep compose and Dockerfile changes minimal and focused on the requested behavior.
- Preserve existing network/depends_on/healthcheck intent unless the task explicitly requires lifecycle changes.

## Verification Checklist

- `docker ps` shows all three containers running.
- `curl http://localhost:5241/_health` responds healthy.
- `curl http://localhost:5175/health` responds healthy.

## Expansion Notes

- Add workflow updates under `Core Workflow` and keep task names exact.
- Add service-specific runtime behavior under `Ports and Services` with concrete port/container values.
- Keep safety boundaries explicit in `Safety` when new data volumes or stateful services are added.
