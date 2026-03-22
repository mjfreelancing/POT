---
applyTo: "Source/Docker/**"
---

# Docker Workflow Instructions

## Core Rules

### Lifecycle

- Prefer explicit, repeatable command sequences.
- Keep compose command usage explicit and repeatable.
- Validate service health after lifecycle changes.

### Safety

- Treat runtime volume/data directories as protected unless explicitly requested.
- Keep Dockerfile/compose edits minimal and focused on requested behavior.

### Validation

- Verify containers are running.
- Verify configured API and client health endpoints respond.

## Expansion Notes

- Keep task names and compose file paths in project-specific overlays or token files.
  - POT: Prefer VS Code tasks `docker-start-pot-client-server` and `docker-stop-pot-client-server` for full-stack lifecycle.
  - POT: Primary compose file is `Source/Docker/docker-compose-client-server.yml`.
  - POT: Direct compose commands should load `--env-file .env --env-file .env.development`.
  - POT: Expected host ports are client `5175`, API `5241`, and Postgres `5444`.
  - POT: Expected containers are `pot-react`, `pot-aspnet`, and `pot-postgres`.
  - POT: API health endpoint is `http://localhost:5241/_health`; frontend health endpoint is `http://localhost:5175/health`.
  - POT: Treat `Source/Docker/postgres-data/**` as protected runtime data and preserve existing `network`, `depends_on`, and `healthcheck` intent unless explicitly asked.
