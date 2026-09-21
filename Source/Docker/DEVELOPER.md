# Docker Developer Guide

Technical reference for the POT Docker stack: how the three containers are wired together, how the
images are built, and how the stack behaves locally and on Azure.

> **Setting up for the first time?** Follow `Docs/DOCKER-SETUP.md`. This guide explains how the
> configuration works, so reach for it when you need to change or debug it.

## Table of Contents

- [Overview](#overview)
- [Container Architecture](#container-architecture)
- [Development Workflow](#development-workflow)
- [Docker Compose Configuration](#docker-compose-configuration)
- [Images and Dockerfiles](#images-and-dockerfiles)
- [Environment Variables](#environment-variables)
- [Networking](#networking)
- [Data Persistence](#data-persistence)
- [Health Checks](#health-checks)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)
- [Additional Resources](#additional-resources)

---

## Overview

POT runs as three containers defined in a single compose file:

| Service    | Container name | Image                             | Host port | Container port |
| ---------- | -------------- | --------------------------------- | --------- | -------------- |
| `postgres` | `pot-postgres` | `pot-prodlike-postgres`           | `5444`    | `5432`         |
| `server`   | `pot-aspnet`   | `pot-server:${IMAGE_TAG:-latest}` | `5241`    | `5241`         |
| `client`   | `pot-react`    | `pot-client:${IMAGE_TAG:-latest}` | `5175`    | `80`           |

**Compose file:** `Source/Docker/docker-compose-client-server.yml`

**Project name:** `pot-prodlike`, set by `COMPOSE_PROJECT_NAME` in `Source/Docker/.env`. The project
name prefixes the compose network and any image the compose file builds without an explicit
`image:` key, which is why the database image is `pot-prodlike-postgres` while its container is
named `pot-postgres`.

The stack is normally driven by the VS Code tasks in `.vscode/tasks.json`, which pass
`--env-file .env --env-file .env.development` to compose. `Start-ProdlikePot.ps1` is the standalone
equivalent for terminals outside VS Code.

### Other Docker stacks in the repository

These are separate compose files, not part of the stack described here:

| Location            | Project name   | Postgres host port | Purpose                             |
| ------------------- | -------------- | ------------------ | ----------------------------------- |
| `Source/Docker`     | `pot-prodlike` | `5444`             | Full prodlike stack (this document) |
| `Data/Local Docker` | `pot-local`    | `5432`             | Database for dev-mode client/server |
| `Data/E2E Docker`   | `pot-e2e`      | `5432`             | Isolated database for E2E runs      |

Only one stack should run at a time: `pot-local` and `pot-e2e` both bind port `5432`, and both
`pot-prodlike` and the local dev client use port `5175`. See `Data/README.md`.

### Key terms

- **Bind mount** — a host folder mapped into a container. The database uses `./postgres-data`, so
  the data lives in the repository folder and outlives the container.
- **Named volume** — Docker-managed storage with a Docker-assigned location. This stack does not use
  one.
- **`IMAGE_TAG`** — the tag compose appends to the server and client images. The VS Code tasks set it
  to a timestamp (`yyyyMMdd-HHmmss`) and also re-tag the result as `latest`.
- **prodlike** — the local stack runs the server with `ASPNETCORE_ENVIRONMENT=Production` so it
  behaves like a deployed environment.

---

## Container Architecture

### Dependencies and startup order

```
client ──depends_on (no condition)──▶ server ──depends_on: service_healthy──▶ postgres
```

Startup happens in the opposite direction to reading order — postgres must be ready first:

1. `postgres` starts and reports healthy through `pg_isready`.
2. `server` starts once postgres is healthy. Its entrypoint (`Docker/Server/entrypoint.sh`) runs EF
   Core migrations, then starts the API.
3. `client` starts once the `server` **container** is running. There is no `service_healthy`
   condition here because the server service has no health check.

Because the client waits only for the server container to exist, the first page load can happen
while the API is still applying migrations. Refresh if the first request fails.

### Ports

**Local (compose):**

- Client: `http://localhost:5175` → container port `80` (nginx)
- Server: `http://localhost:5241` → container port `5241` (Kestrel)
- PostgreSQL: `localhost:5444` → container port `5432`

The postgres and client host ports deliberately differ from their container ports so they do not
clash with a locally installed PostgreSQL or the Vite dev server.

**Azure (Container Apps):** each container listens on its normal port (client `80`, API `5241`) and
container-app ingress terminates HTTPS and routes `/api/*` to the API container. See
`Docs/Azure/Azure-Deployment-Notes.md`.

---

## Development Workflow

### Prerequisites

- Docker Desktop running.
- `Source/Docker/.env.development` present. It is git-ignored, so each developer creates their own
  (see [Environment Variables](#environment-variables) for the values it needs).
- `Source/Docker/postgres-data/` exists. `Start-ProdlikePot.ps1` creates it automatically; when
  starting compose by hand, create it first:

  ```powershell
  cd Source/Docker
  New-Item -ItemType Directory -Force -Path postgres-data
  ```

### Start and stop with VS Code tasks (preferred)

Tasks are defined in `.vscode/tasks.json` and run from the VS Code Task menu
(`Ctrl+Shift+P` → **Run Task**). Both images set `no_cache: true` in the compose file, so every build
task recompiles from scratch.

| Workflow                                | Start task                   | Stop task                   |
| --------------------------------------- | ---------------------------- | --------------------------- |
| Full stack (client + server + postgres) | `docker-start-client-server` | `docker-stop-client-server` |
| Server + postgres                       | `docker-start-server`        | `docker-stop-server`        |
| Client only                             | `docker-start-client`        | `docker-stop-client`        |

The client-only tasks assume the server is already running. Two further Docker-related tasks exist:
`docker-prune-all-unused-images` (removes unused images) and the `docker-start-e2e-postgres` /
`docker-stop-e2e-postgres` pair, which manages the independent E2E stack in `Data/E2E Docker`.

### Start with the PowerShell script

```powershell
cd Source/Docker
.\Start-ProdlikePot.ps1
```

The script creates `postgres-data` if needed, builds all images with a timestamp tag, re-tags them as
`latest`, then starts the stack.

### Start with Docker Compose directly

Run these from `Source/Docker`. Both `--env-file` flags matter: `.env` supplies the project and
database names, while `.env.development` supplies the database credentials, JWT keys, SMTP settings,
CORS origins and platform admin IDs that the compose file substitutes into the containers. Leaving
`.env.development` out makes those values empty, and the API fails its startup validation.

```powershell
cd Source/Docker

docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d

# Follow all logs, or just one service
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml logs -f
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml logs -f server

# Stop and remove the containers (the database data stays on disk)
docker-compose -f docker-compose-client-server.yml down
```

Do not add `-p pot`. The project name comes from `.env`, and overriding it points compose at a
different project whose containers collide with the existing `container_name` values.

### Making changes

| Change                | What to do                                                                                      |
| --------------------- | ----------------------------------------------------------------------------------------------- |
| Client (React)        | Edit `Source/Client/pot-react/`, then rebuild the client container                              |
| Server (C#)           | Edit `Source/Server/`, then rebuild the server container                                        |
| Database schema       | Add a migration, then rebuild the server container — migrations run at container startup        |
| Environment variables | Edit `.env.development`, then recreate the container (client build args need a rebuild instead) |

```powershell
# Rebuild one service
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d server
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d client
```

Add migrations from `Source/Server`:

```powershell
dotnet ef migrations add <Name> --project Pot.Data --startup-project Pot.Data.Migrations
```

> Both images set `no_cache: true`, so every rebuild recompiles from scratch and takes minutes
> rather than seconds. Each build also leaves a new timestamped image behind; clean them up
> periodically with the `docker-prune-all-unused-images` task.

---

## Docker Compose Configuration

**File:** `Source/Docker/docker-compose-client-server.yml`

**Services:** `postgres`, `server`, `client` · **Network:** `pot-network` (bridge) · **Data:** bind
mount `./postgres-data`

The excerpts below are abridged for readability. The compose file is the source of truth; use these
to understand the intent, not to copy-paste wholesale.

### postgres

```yaml
postgres:
  build:
    context: Postgres
    dockerfile: Dockerfile
  container_name: pot-postgres
  restart: always
  ports:
    - "5444:5432"
  environment:
    - POSTGRES_USER=${POSTGRES_USER}
    - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    - POSTGRES_DB=${POSTGRES_DB}
  volumes:
    - ./postgres-data:/var/lib/postgresql/data
  healthcheck:
    test: "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"
    interval: 5s
    timeout: 5s
    retries: 10
  networks:
    - pot-network
```

Built from `Source/Docker/Postgres/Dockerfile` (`FROM postgres:13`). Credentials are substituted
from the env files rather than hard-coded, so the container and the server always agree on them.

### server

```yaml
server:
  build:
    context: ..
    dockerfile: Docker/Server/Dockerfile
    no_cache: true
  image: pot-server:${IMAGE_TAG:-latest}
  container_name: pot-aspnet
  restart: always
  depends_on:
    postgres:
      condition: service_healthy
  ports:
    - "5241:5241"
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - ASPNETCORE_URLS=http://+:5241
    # DATABASE__*, JWT__*, SMTP__*, CORS__*, PLATFORMADMIN__* follow.
    # See Environment Variables for the full list and where each value comes from.
  networks:
    - pot-network
```

`${IMAGE_TAG:-latest}` falls back to `latest` when `IMAGE_TAG` is unset, which is what happens when
you run compose by hand instead of through the VS Code tasks. `no_cache: true` forces a full rebuild
on every `up --build`.

### client

```yaml
client:
  build:
    context: ..
    dockerfile: Docker/Client/Dockerfile
    no_cache: true
    args:
      - NODE_ENV=production
  image: pot-client:${IMAGE_TAG:-latest}
  container_name: pot-react
  restart: always
  depends_on:
    - server
  ports:
    - "5175:80"
  healthcheck:
    test: "wget -q --spider http://server:5241/_health || exit 1"
    interval: 10s
    timeout: 5s
    retries: 3
    start_period: 5s
  networks:
    - pot-network
```

`depends_on: server` intentionally has no `service_healthy` condition — the server has no health
check. The `NODE_ENV` build arg is passed for clarity; the Dockerfile sets `ENV NODE_ENV=production`
itself.

### network

```yaml
networks:
  pot-network:
    driver: bridge
```

A custom bridge network gives every service DNS resolution by container name, which is how the
server finds `pot-postgres` and how the client finds `pot-aspnet`. Compose names the network
`pot-prodlike_pot-network` (project name + network key).

---

## Images and Dockerfiles

Both images are multi-stage builds: a full toolchain compiles the application, then a minimal runtime
image is produced without build tools or source code.

### Server — `Source/Docker/Server/Dockerfile`

**`build` stage** (`mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine`):

1. Copies `Server/` into the image.
2. Restores `Pot.AspNetCore` and `Pot.Data.Migrations` and their transitive dependencies.
3. Publishes the API to `/app/server` and the migrations tool to `/app/migrations`, both with
   `-p:UseAppHost=false` so they run as `dotnet <dll>`.

**Runtime stage** (`mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine`):

1. Installs `postgresql-client` (`pg_isready`, `psql`, `pg_dump`) and `tzdata` (required by
   `TimeZoneInfo.FindSystemTimeZoneById`).
2. Creates an `appgroup`/`appuser` pair with UID/GID 1000.
3. Copies both publish outputs plus `Docker/Server/entrypoint.sh`.
4. Runs `dos2unix` on the entrypoint (so a Windows checkout cannot break it), marks it executable,
   and gives `appuser` ownership of `/app`.
5. Switches to `USER appuser`, documents port 5241 with `EXPOSE`, and starts
   `ENTRYPOINT ["/bin/sh", "./entrypoint.sh"]`.

Things worth knowing:

- **`ARG DOTNET_VERSION=10.0`** is the single place to change the .NET version. Override a single
  build with `--build-arg DOTNET_VERSION=11.0`, or change the default in the Dockerfile.
- **Migrations live in the same container.** `entrypoint.sh` runs the migrations tool and then the
  API in the foreground. It uses `set -e`, so a failed migration stops the container instead of
  starting an API against an out-of-date schema.
- **`EXPOSE 5241` is documentation only.** The real listen address comes from `ASPNETCORE_URLS`.

### Client — `Source/Docker/Client/Dockerfile`

**`build` stage** (`node:24-alpine`):

1. Copies `Client/pot-react/package*.json` and runs `npm ci`.
2. Copies the rest of `Client/pot-react/`.
3. Sets `ENV NODE_ENV=production`.
4. Exports the Vite build args, if supplied, and runs `npm run build`.

**Runtime stage** (`nginx:alpine`): copies `/app/dist` to `/usr/share/nginx/html`, copies the
selected nginx config to `/etc/nginx/conf.d/default.conf`, and runs nginx in the foreground.

Things worth knowing:

- **Dependency layer caching.** `package*.json` is copied and `npm ci` is run before the source is
  copied, so dependency installation is reused unless dependencies change.
- **Node 24** is the minimum for the current toolchain (Vite 8, react-router 8, ESLint 10,
  `@testing-library/jest-dom` 7).
- **Build-time configuration.** `ARG VITE_API_BASE_URL` and `ARG VITE_API_TIMEOUT_MS` have no
  defaults. The Dockerfile only exports them when they are non-empty:

  ```dockerfile
  RUN if [ -n "$VITE_API_BASE_URL" ]; then \
    export VITE_API_BASE_URL=$VITE_API_BASE_URL; \
    export VITE_API_TIMEOUT_MS=$VITE_API_TIMEOUT_MS; \
    fi && \
    npm run build
  ```

  When they are omitted, Vite falls back to the client's own environment files —
  `Source/Client/pot-react/.env.production` sets `VITE_API_BASE_URL=/api` and
  `VITE_API_TIMEOUT_MS=30000`.

- **`ARG NGINX_CONFIG=nginx.conf`** selects which config is copied to nginx. `nginx.conf` is used
  locally, `nginx.azure.conf` on Azure.
- **`nginx:alpine` already runs as a non-root user** (the `nginx` user), so no user setup is needed.

> Vite variables are embedded in the bundle at build time. Changing them requires a rebuild, not a
> container restart.

### Nginx configurations

**Development — `Source/Docker/Client/nginx.conf`**

- Serves the SPA with `try_files $uri $uri/ /index.html`, so client-side routes survive a refresh.
- Proxies `/api/` to `http://server:5241/api/` with 60s connect/read/send timeouts.
- Sets PWA cache headers: `index.html`, `sw.js` and `manifest.webmanifest` are `no-cache`, while
  `/assets/` is cacheable for a year because asset filenames are hashed.
- Returns `healthy` from `/health`, so the container can be checked from the host with
  `curl http://localhost:5175/health`.

**Azure — `Source/Docker/Client/nginx.azure.conf`**

Same static-file and PWA behaviour, **without** the `/api/` proxy. Container-app ingress routes
`/api/*` to the API container, and the SPA calls the absolute API URL that was baked in at build
time through `VITE_API_BASE_URL`.

---

## Environment Variables

Compose reads two env files, passed in order so the later file wins:

```powershell
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up
```

### `.env` (committed)

Non-secret values shared by every environment:

```properties
COMPOSE_PROJECT_NAME=pot-prodlike
POSTGRES_DB=Pot
```

### `.env.development` (git-ignored — create it locally)

`Source/Docker/.env.development` is not in source control (`.gitignore` includes `*.development*`),
so each developer creates their own. It supplies the values the compose file substitutes into the
containers:

```properties
# Database credentials
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<local password>

# JWT (issuer and audience must match how the API is reached)
JWT_ISSUER=http://localhost:5241
JWT_AUDIENCE=http://localhost:5241
JWT_SECRET_KEY=<long random value>

# SMTP - required, the API validates these at startup
SMTP_HOST=<smtp host>
SMTP_PORT=<smtp port>
SMTP_REQUIRE_TLS=<true|false>
SMTP_AUTH_USERNAME=<username>
SMTP_AUTH_PASSWORD=<password>
SMTP_FROM_NAME=POT - Do Not Reply
SMTP_FROM_ADDRESS=<from address>

# CORS - must include the browser origin you use
CORS_ALLOWED_ORIGINS=http://localhost:5175

# Platform admin user RowIds (comma-separated, may be empty)
PLATFORM_ADMIN_USERIDS=
```

Notes:

- The API validates SMTP settings at startup, so those values cannot be blank even if you never send
  email locally.
- `JWT_SECRET_KEY` signs and verifies tokens; use a long random value and never reuse a production
  key locally.
- To find a user's `RowId` for `PLATFORM_ADMIN_USERIDS`, query the database:
  `SELECT "RowId" FROM "User" WHERE "Username" = '<username>'`.

### Variables compose passes to the server

| Container variable                                                 | Value                       | Maps to in `appsettings.json` |
| ------------------------------------------------------------------ | --------------------------- | ----------------------------- |
| `ASPNETCORE_ENVIRONMENT`                                           | fixed: `Production`         | ASP.NET environment           |
| `ASPNETCORE_URLS`                                                  | fixed: `http://+:5241`      | Kestrel listen address        |
| `DATABASE__HOST`                                                   | fixed: `pot-postgres`       | `Database:Host`               |
| `DATABASE__USERNAME`                                               | `${POSTGRES_USER}`          | `Database:Username`           |
| `DATABASE__PASSWORD`                                               | `${POSTGRES_PASSWORD}`      | `Database:Password`           |
| `DATABASE__NAME`                                                   | `${POSTGRES_DB}`            | `Database:Name`               |
| `DATABASE__PORT`                                                   | fixed: `5432`               | `Database:Port`               |
| `DATABASE__SSLMODE`                                                | fixed: `Disable`            | `Database:SSLMode`            |
| `JWT__ISSUER`                                                      | `${JWT_ISSUER}`             | `Jwt:Issuer`                  |
| `JWT__AUDIENCE`                                                    | `${JWT_AUDIENCE}`           | `Jwt:Audience`                |
| `JWT__SECRETKEY`                                                   | `${JWT_SECRET_KEY}`         | `Jwt:SecretKey`               |
| `SMTP__HOST`, `SMTP__PORT`, `SMTP__REQUIRETLS`                     | `${SMTP_*}`                 | `Smtp:*`                      |
| `SMTP__AUTHENTICATION__USERNAME`, `SMTP__AUTHENTICATION__PASSWORD` | `${SMTP_AUTH_*}`            | `Smtp:Authentication:*`       |
| `SMTP__FROM__NAME`, `SMTP__FROM__ADDRESS`                          | `${SMTP_FROM_*}`            | `Smtp:From:*`                 |
| `CORS__ALLOWEDORIGINS`                                             | `${CORS_ALLOWED_ORIGINS}`   | `Cors:AllowedOrigins`         |
| `PLATFORMADMIN__USERIDS`                                           | `${PLATFORM_ADMIN_USERIDS}` | `PlatformAdmin:UserIds`       |

The double underscore is ASP.NET Core's separator for nested configuration keys, so
`DATABASE__HOST` sets `Database:Host`. `DATABASE__PORT` is the **container** port (5432), not the
host port (5444) — the server reaches postgres over the compose network.

### Client build arguments

The client has no runtime configuration: Vite embeds these values at build time.

| Build arg             | Default                                          | Notes                                                           |
| --------------------- | ------------------------------------------------ | --------------------------------------------------------------- |
| `NGINX_CONFIG`        | `nginx.conf` (Dockerfile default)                | Use `nginx.azure.conf` for Azure                                |
| `VITE_API_BASE_URL`   | none — falls back to `.env.production` (`/api`)  | Absolute API URL; Azure uses `https://api.payontime.com.au/api` |
| `VITE_API_TIMEOUT_MS` | none — falls back to `.env.production` (`30000`) | Request timeout for slow requests such as Azure cold starts     |

`docker-compose-client-server.yml` passes only `NODE_ENV=production`, so local builds rely on the
`.env.production` fallbacks. The `azure-client-build-and-deploy` task passes all three explicitly —
see [Production Deployment](#production-deployment).

---

## Networking

### The compose network

```yaml
networks:
  pot-network:
    driver: bridge
```

All three services join `pot-network` (compose names the actual network
`pot-prodlike_pot-network`). Docker's embedded DNS resolves container names on that network, so
services address each other by name instead of IP.

### Who talks to whom

| From             | To       | Address                 | Notes                                   |
| ---------------- | -------- | ----------------------- | --------------------------------------- |
| Host machine     | client   | `http://localhost:5175` | published port                          |
| Host machine     | server   | `http://localhost:5241` | published port                          |
| Host machine     | postgres | `localhost:5444`        | published port (container port is 5432) |
| Client container | server   | `http://server:5241`    | nginx proxy and health check            |
| Server container | postgres | `pot-postgres:5432`     | internal port, not the host port        |

`localhost` inside a container always means that container, which is why the client's health check
targets `server:5241` rather than `localhost:5241` — nothing is listening on 5241 in the client.

### Relationships worth remembering

- The browser only ever talks to port 5175: the client's nginx proxies `/api/` to
  `http://server:5241/api/`.
- The server reaches postgres using `DATABASE__HOST=pot-postgres` and `DATABASE__PORT=5432` (see
  [Environment Variables](#environment-variables)).
- Only client, server and postgres publish host ports; nothing else is exposed.

---

## Data Persistence

PostgreSQL data lives in a **bind mount**, not a named volume:

```yaml
volumes:
  - ./postgres-data:/var/lib/postgresql/data
```

`Source/Docker/postgres-data/` is an ordinary folder on your machine (git-ignored by
`**/postgres-data/`). Consequences worth knowing:

- The data survives `docker-compose down` **and** `docker-compose down -v`. The `-v` flag only
  removes named and anonymous volumes, and this stack has neither.
- To reset the database you must stop the stack and delete the folder:

  ```powershell
  cd Source/Docker
  docker-compose -f docker-compose-client-server.yml down
  Remove-Item -Recurse -Force ./postgres-data
  .\Start-ProdlikePot.ps1
  ```

  Migrations and seed data run again on the next start, so the database comes back empty.

- `docker volume prune` does not touch this data, and `docker volume inspect pot_postgres-data`
  fails with "no such volume" — there is no volume to inspect.
- Because the folder is created by Docker on first use, ensure it exists before starting if you want
  to control its permissions (see [Development Workflow](#development-workflow)).

### Backup and restore

`pg_dump` and `psql` are available inside the postgres container:

```powershell
# Dump to a file inside the container, then copy it out
docker exec pot-postgres pg_dump -U postgres -d Pot -f /tmp/pot-backup.sql
docker cp pot-postgres:/tmp/pot-backup.sql .\pot-backup.sql

# Restore into a fresh database
docker cp .\pot-backup.sql pot-postgres:/tmp/pot-backup.sql
docker exec pot-postgres psql -U postgres -d Pot -f /tmp/pot-backup.sql
```

`Data/README.md` describes where database and application backups are kept for the various
environments.

---

## Health Checks

The stack defines two health checks. Both are in the compose file; this section explains what they
mean in practice.

### postgres

`pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}`, checked every 5s with a 5s timeout and 10
retries. Retries × interval means postgres has roughly 50 seconds to report healthy before Docker
marks it unhealthy.

This is the check that matters most: the `server` service waits on it
(`depends_on: postgres: condition: service_healthy`), so a database that never becomes healthy keeps
the API from starting.

### client

The client's check runs **inside the client container** and requests
`http://server:5241/_health`: every 10s, 5s timeout, 3 retries, 5s start period. Targeting the
compose service name verifies both network reachability and that the API answers.

To check nginx itself from the host instead:

```powershell
curl http://localhost:5175/health   # returns "healthy"
```

### server

The `server` service has **no health check**, which has two visible consequences:

- `client` uses a plain `depends_on: server`, so the client container can start while the API is
  still applying migrations.
- `docker inspect --format='{{.State.Health.Status}}' pot-aspnet` returns nothing, because there is
  no health state to report.

The API itself does expose health endpoints (`MapHealthChecks` in `Pot.AspNetCore/Program.cs`):
`/_health` (liveness) and `/_health/ready` (readiness, including database checks). The client health
check already relies on `/_health`; these endpoints are also useful when a container app revision is
deployed on Azure.

If a server health check were added, the runtime image supports the usual Alpine probe — the
`aspnet:10.0-alpine` image ships BusyBox `wget` (but no `curl`):

```yaml
healthcheck:
  test:
    ["CMD-SHELL", "wget -q --spider http://localhost:5241/_health || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

A migration run of a few seconds sits inside that 40s start period, so the value is a reasonable
starting point if you decide to add the check.

### Checking health status

```powershell
# Container status, including health where a check exists
docker ps

# Health of a specific container
docker inspect --format='{{.State.Health.Status}}' pot-postgres
docker inspect --format='{{.State.Health.Status}}' pot-react

# Recent health probe output
docker inspect --format='{{json .State.Health}}' pot-postgres
```

---

## Production Deployment

Production runs on Azure Container Apps, with images published to GitHub Container Registry. The
`azure-server-build-and-deploy` and `azure-client-build-and-deploy` tasks wrap the commands below;
they run from `Source/` and use `--no-cache`.

### Pushing images

**Server:**

```powershell
cd Source
docker build --no-cache -t ghcr.io/mjfreelancing/pot-server:latest -f Docker/Server/Dockerfile .
docker push ghcr.io/mjfreelancing/pot-server:latest
```

**Client** — uses the Azure nginx config and bakes in the public API URL:

```powershell
cd Source
docker build --no-cache `
  --build-arg NGINX_CONFIG=nginx.azure.conf `
  --build-arg VITE_API_BASE_URL=https://api.payontime.com.au/api `
  --build-arg VITE_API_TIMEOUT_MS=30000 `
  -t ghcr.io/mjfreelancing/pot-client:latest `
  -f Docker/Client/Dockerfile .
docker push ghcr.io/mjfreelancing/pot-client:latest
```

### Image tagging

| Context                | Tags produced                                          |
| ---------------------- | ------------------------------------------------------ |
| Local tasks and script | `<image>:<yyyyMMdd-HHmmss>` plus `<image>:latest`      |
| Azure deploy tasks     | `ghcr.io/mjfreelancing/pot-server:latest` (and client) |

Timestamped tags make a local build identifiable; they also accumulate on disk and are worth clearing
with the `docker-prune-all-unused-images` task.

### How Azure differs from local

- **No `/api` proxy** — container-app ingress routes `/api/*` to the API container, so the SPA uses
  the absolute URL baked in at build time.
- **No compose stack** — postgres is not part of these images; connection details come from the
  container app's environment variables.
- **Ingress ports** — port 80 for the client container, 5241 for the API container.
- **Migrations still run at startup**, because the image entrypoint is unchanged.

Deployment steps (container app configuration, ingress, secrets, custom domains) are in
`Docs/Azure/Azure-Deployment-Notes.md`.

---

## Troubleshooting

### A container won't start

```powershell
docker logs pot-postgres
docker logs pot-aspnet
docker logs pot-react
```

Common causes:

- **Port already in use** — check `netstat -an | findstr 5444` (postgres), `findstr 5241` (API) or
  `findstr 5175` (client).
- **Missing environment values** — starting compose without
  `--env-file .env --env-file .env.development` leaves the `${...}` placeholders empty, and the API
  fails its startup validation.
- **Database never becomes healthy** — check the postgres logs first; the server waits for it.
- **Build failure** — read the Dockerfile output. Both images build with `no_cache: true`, so the
  full build runs every time.

### The database rejects connections

```powershell
# Inside the server container (Alpine image: use sh, not bash). psql is already installed.
docker exec -it pot-aspnet sh
psql -h pot-postgres -p 5432 -U postgres -d Pot   # enter POSTGRES_PASSWORD when prompted

# Or connect from inside the postgres container
docker exec -it pot-postgres psql -U postgres -d Pot
```

Things to check:

- Inside the network the host is `pot-postgres` and the port is `5432`.
- From the host the port is `5444`.
- The database name is `Pot` (capital P), as set by `POSTGRES_DB` in `.env`.
- The server reads `DATABASE__HOST`, `DATABASE__USERNAME`, `DATABASE__PASSWORD`, `DATABASE__NAME`,
  `DATABASE__PORT` and `DATABASE__SSLMODE` — there is no connection-string setting to fix. See
  [Environment Variables](#environment-variables).

### The client can't reach the API

```powershell
docker exec -it pot-react sh
wget -q --spider http://server:5241/_health && echo reachable
```

If that fails, the containers are not resolving each other - confirm both are attached to
`pot-prodlike_pot-network` with `docker network inspect pot-prodlike_pot-network`. If the API URL
baked into the bundle is wrong, rebuild the client. See
[Client build arguments](#client-build-arguments).

### I need to reset the database

Deleting `Source/Docker/postgres-data/` is what resets the data; `docker-compose down -v` does not.
See [Data Persistence](#data-persistence).

### Rebuild after changing code

```powershell
cd Source/Docker
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d server
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up --build -d client
```

---

## Best Practices

### Container design

1. **Use multi-stage builds** so build tools never reach the runtime image.
2. **Copy only what the build needs** — the server copies `Server/`, the client copies
   `Client/pot-react/`.
3. **Keep `.dockerignore` current** (`Source/.dockerignore`) so `node_modules`, `bin`, `obj` and local
   env files stay out of the build context.
4. **Run as a non-root user** (where possible) — the server uses `appuser`; `nginx:alpine` already
   runs as the `nginx` user.
5. **Pin base image versions** and bump them deliberately (`DOTNET_VERSION`, `postgres:13`,
   `node:24-alpine`).

### Environment variables and secrets

1. **Don't commit secrets** — `.env.development` is git-ignored for exactly this reason.
2. **Keep `.env` for non-secret, shared values** such as the project and database names.
3. **Use platform secret storage in production** rather than committed files or build args.
4. **Keep startup validation** — the API fails fast on missing SMTP/JWT settings, which is what
   surfaces a misconfigured env file immediately instead of at first use.

### Networking and data

1. **Use service/container names** for container-to-container traffic, never `localhost`.
2. **Publish only the ports that need publishing** (5175, 5241, 5444).
3. **Remember the data is a bind mount** — back it up, and don't expect `down -v` to clean it up.
4. **Run one stack at a time** — the local, E2E and prodlike stacks compete for ports 5432 and 5175.

### Health checks

1. **Add a health check where another service depends on readiness** (postgres already has one).
2. **Choose intervals and timeouts deliberately** — retries × interval is how long a slow service can
   take before it is marked unhealthy.
3. **Prefer `depends_on` with `condition: service_healthy`** when the consumer genuinely needs the
   dependency to be ready; use a plain `depends_on` only when it does not.

---

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Nginx Docker Hub](https://hub.docker.com/_/nginx)

In this repository:

- `Docs/DOCKER-SETUP.md` — step-by-step local setup with Docker
- `Docs/LOCAL-SETUP.md` — running the client and server without Docker
- `Docs/Azure/Azure-Deployment-Notes.md` — Azure deployment detail
- `Data/README.md` — the three local database environments
- `Source/Server/DEVELOPER.md` — backend patterns
- `Source/Client/pot-react/DEVELOPER.md` — frontend patterns
