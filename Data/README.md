# POT Data

This folder contains persistent data and supporting files for the three local database environments used during development and testing.

---

## Container Overview

Three Docker-based postgres containers exist across the project. Only **one** should be running at a time, since `pot-local` and `pot-e2e` both bind to the same postgres port, and all clients (local dev and full docker stack) use port `5175`.

| Container            | Project name   | Docker Desktop label | Postgres host port | Purpose                                      |
| -------------------- | -------------- | -------------------- | ------------------ | -------------------------------------------- |
| `pot-local-postgres` | `pot-local`    | `postgres`           | `5432`             | Local postgres for dev-mode client/server    |
| `pot-e2e-postgres`   | `pot-e2e`      | `postgres`           | `5432`             | Isolated postgres for E2E baseline data prep |
| `pot-postgres`       | `pot-prodlike` | `pot-postgres`       | `5444`             | Full docker stack in `Source/Docker/`        |

> **Project names**: `pot-local` and `pot-e2e` are set via the `name:` directive at the
> top of their respective `docker-compose.yml` files. `pot-prodlike` is set via
> `COMPOSE_PROJECT_NAME=pot-prodlike` in `Source/Docker/.env`, which the VS Code tasks
> pass to docker-compose with `--env-file .env`.

> **Port isolation note**: `pot-local` and `pot-e2e` both use port `5432` for postgres.
> The full docker stack in `Source/Docker/` uses port `5444` for postgres but also binds
> the client to port `5175` — the same port used by the local dev client. Only one
> environment should be active at a time to avoid both postgres and client port conflicts.

> **Docker Desktop display note**: Docker Desktop strips the project-name prefix from
> `container_name` when rendering the project tree. For example, `pot-e2e-postgres` under
> the `pot-e2e` project has `pot-e2e-` stripped and displays as `postgres`. Containers
> whose `container_name` does not begin with `{project-name}-` are shown with their full
> name. The actual container name used by `docker exec`, `docker ps`, and all CLI commands
> is always the full `container_name` defined in the compose file. Run
> `docker ps -a --format "table {{.Names}}\t{{.Status}}"` to confirm the real names.

---

## Azure

**Location**: `Azure/`

Contains data exported from or backed up against the **Azure production environment**:

- `Azure/Exports/` — application-level exports produced via the **Export** feature inside the POT app running in Azure.
- `Azure/Backups/` — database-level backups taken manually via **DBeaver**.

### Usage

This folder is a storage location only — there is no Docker container here. To work with Azure data locally:

1. Stop `pot-e2e` and the full docker stack (`pot-prodlike`) if running.
2. Ensure `pot-local` is running (see [Local Docker](#local-docker)).
3. Restore the desired export or backup into the `pot-local-postgres` container.
4. Start the local client and server applications — they connect automatically on `localhost:5432`.

All data is persisted between container deletions and recreations via the bind-mounted `postgres-data` folder in `Local Docker/`.

---

## E2E Docker

**Location**: `E2E Docker/`

Contains a Docker Compose setup and persistent postgres data for the **pot-e2e** container. This container is used exclusively for **preparing and maintaining the baseline data for E2E tests**.

- `E2E Docker/postgres-data/` — bind-mounted postgres data directory. Survives `docker compose down`.
- `E2E Docker/Backups/` — manual DBeaver backups of the E2E database state.

### Connection details

| Setting   | Value              |
| --------- | ------------------ |
| Host      | `localhost`        |
| Port      | `5432`             |
| Database  | `Pot`              |
| Username  | `postgres`         |
| Password  | `password`         |
| Container | `pot-e2e-postgres` |

### Starting and stopping

```powershell
# Start
.\Start-E2EPot.ps1

# Stop (data preserved)
docker compose -p pot-e2e stop

# Remove container (data preserved)
docker compose -p pot-e2e down
```

### Viewing data via the POT app

This container only provides a postgres database. To browse the data through the POT application:

1. Stop `pot-local` and the full docker stack (`pot-prodlike`) if running — they share port `5432`.
2. Start `pot-e2e`: `.\Start-E2EPot.ps1`
3. Start the local client and server applications — they connect automatically on `localhost:5432`.

### Exporting baseline seed data for E2E tests

The E2E test suite loads `Source/Client/pot-react/e2e/seed/baseline.sql` before each run. When the canonical test users or schema change, this file must be regenerated from the `pot-e2e-postgres` container.

**Preferred — automated script** (from `Source/Client/pot-react/e2e/scripts/`):

```powershell
# PowerShell 7 required. If you are in Windows PowerShell (5.x), run pwsh first.
pwsh ./test-e2e-seed-export.ps1
```

**Manual terminal alternative** (from the workspace root):

```powershell
# Ensure pot-e2e-postgres is running first
docker exec -e PGPASSWORD=password pot-e2e-postgres pg_dump `
  --data-only `
  -t '"Site"' `
  -t '"User"' `
  -t '"UserRole"' `
  -U postgres `
  -d Pot `
  | Out-File -FilePath Source/Client/pot-react/e2e/seed/baseline.sql -Encoding UTF8
```

Or using bash-style syntax (e.g. from Git Bash or WSL):

```bash
docker exec -e PGPASSWORD=password pot-e2e-postgres pg_dump \
  --data-only \
  -t '"Site"' \
  -t '"User"' \
  -t '"UserRole"' \
  -U postgres \
  -d Pot \
  > Source/Client/pot-react/e2e/seed/baseline.sql
```

> See `Source/Client/pot-react/e2e/README.md` for full details on when to regenerate
> seed data and what the canonical users should be.

### Data persistence

Data is stored in `E2E Docker/postgres-data/` on the host filesystem as a bind mount — it is **never** removed by `docker compose down` (with or without `-v`). Delete the `postgres-data` folder manually to wipe the database.

---

## Local Docker

**Location**: `Local Docker/`

Contains a Docker Compose setup and persistent postgres data for the **pot-local** container. This container provides an isolated postgres database for use **while running the POT client and server applications locally** (dev mode, not the full docker stack).

- `Local Docker/postgres-data/` — bind-mounted postgres data directory. Survives `docker compose down`.
- `Local Docker/Backups/` — manual DBeaver backups.

### Connection details

| Setting   | Value                |
| --------- | -------------------- |
| Host      | `localhost`          |
| Port      | `5432`               |
| Database  | `Pot`                |
| Username  | `postgres`           |
| Password  | `password`           |
| Container | `pot-local-postgres` |

### Starting and stopping

```powershell
# Start
.\Start-LocalPot.ps1

# Stop (data preserved)
docker compose -p pot-local stop

# Remove container (data preserved)
docker compose -p pot-local down
```

### Usage

1. Stop `pot-e2e` and the full docker stack (`pot-prodlike`) if running — they conflict on port `5432` or `5444`.
2. Start `pot-local`: `.\Start-LocalPot.ps1`
3. Start the local client and server applications — they connect automatically on `localhost:5432`.

### Data persistence

Data is stored in `Local Docker/postgres-data/` on the host filesystem as a bind mount — it is **never** removed by `docker compose down` (with or without `-v`). Delete the `postgres-data` folder manually to wipe the database.
