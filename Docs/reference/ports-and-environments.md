# Ports and environments

Every port POT uses, where its value lives, and which values must agree when one changes. This is the
only place the set is described; the guides link here.

## Docker full stack

`Source/Docker/docker-compose-client-server.yml`, started by the `docker-start-client-server` task.

| Service    | Container     | Host  | Container | Owner of the value                                                 |
| ---------- | ------------- | ----- | --------- | ------------------------------------------------------------------ |
| PostgreSQL | `pot-postgres` | 5444  | 5432      | compose `ports`; the image exposes 5432                             |
| API        | `pot-aspnet`  | 5241  | 5241      | compose `ports`, `ASPNETCORE_URLS`, and the server Dockerfile `EXPOSE` |
| Client     | `pot-react`   | 5175  | 80        | compose `ports`, nginx `listen 80`, and the client Dockerfile `EXPOSE` |

Why these numbers:

- **5444** keeps the container off 5432, so a PostgreSQL already installed on the host does not collide
  with it. Inside the stack the API still connects to `postgres:5432`; 5444 is only for reaching the
  database from the host, for example with `psql -h localhost -p 5444` or DBeaver.
- **5241** is the same inside and outside the container, so a command, a log line or a CORS origin needs
  no translation between the two.
- **5175** is a deliberate override of Vite's 5173 default, set in the client's npm scripts rather than in
  `vite.config.ts`. Inside the container nginx listens on the conventional port 80.

Addresses worth knowing in this stack:

| What                  | URL                                    | Notes                    |
| --------------------- | -------------------------------------- | ------------------------ |
| Client                | `http://localhost:5175`                | Login page               |
| Client health         | `http://localhost:5175/health`         | Served by nginx          |
| API liveness          | `http://localhost:5241/_health`        | Runs no checks           |
| API readiness         | `http://localhost:5241/_health/ready`  | Includes the database    |
| API reference UI      | `http://localhost:5241/scalar`         | Development only         |

## Running without Docker

| Process            | Port | Owner of the value                                                     |
| ------------------ | ---- | ---------------------------------------------------------------------- |
| API                | 5242 | `Pot.AspNetCore/Properties/launchSettings.json` (`applicationUrl`)      |
| Vite dev server    | 5175 | `Source/Client/pot-react/package.json` (`vite --port 5175`)             |
| Vite preview       | 5175 | the same file (`vite preview --port 5175`)                              |

The Vite dev server proxies `/api` to `http://localhost:5242` (`Source/Client/pot-react/vite.config.ts`),
so a browser on 5175 reaches an API started with `dotnet run` without a CORS preflight being involved.
The API in this mode is on a different port from the containerised one precisely so both can run at once.

## The local data stacks

`Data/Local Docker` and `Data/Seed Docker` are single-container stacks used to inspect a dump and to
author E2E seed data. Neither is part of the application stack.

| Stack         | Container           | Host | Container | Image         |
| ------------- | ------------------- | ---- | --------- | ------------- |
| Local Docker  | `pot-local-postgres` | 5432 | 5432      | `postgres:13` |
| Seed Docker   | `pot-seed-postgres`  | 5432 | 5432      | `postgres:13` |

Both use the standard port on purpose: they exist to be connected to by ordinary tools, and only one of
them runs at a time.

## End-to-end tests

The dev matrix (`playwright.config.ts`) and the prodlike matrix (`playwright.prod.config.ts`) drive their
own processes.

| What                            | Port  | Where it is set                                        |
| ------------------------------- | ----- | ------------------------------------------------------ |
| API                             | 5242  | the Playwright `webServer` command (`--urls`)           |
| Client                          | 5175  | the Playwright `webServer` (`npm run dev` / `preview`)  |
| PostgreSQL (Testcontainers)     | 55432 | `playwright.globalSetup.ts`                             |
| SMTP sink                       | 1025  | the `webServer` environment (`Smtp__Port`)              |

The harness uses 5242 while the E2E stack is up, so a Docker full stack on 5241 does not have to be
stopped first.

## When a port changes

These values have to agree, and a change to one is a change to all of them:

| Changing                 | Also update                                                                                   |
| ------------------------ | --------------------------------------------------------------------------------------------- |
| API port                 | compose `ports` and `ASPNETCORE_URLS`; the server Dockerfile `EXPOSE`; `JWT_ISSUER` and `JWT_AUDIENCE` in the env file; `CORS_ALLOWED_ORIGINS`; nginx `proxy_pass`; the Vite proxy target; the Playwright `webServer` |
| Client port              | compose `ports`; `CORS_ALLOWED_ORIGINS`; the client npm scripts; the Playwright `webServer` and `baseURL` |
| Database host port       | compose `ports` only, so long as the API keeps using the container port                        |

## Where these numbers also appear

Three agent-facing files state some of these ports deliberately, because an agent should not have to open
a second document to act on a rule: `.github/copilot-instructions.md`, `.github/instructions/docker.instructions.md`
and `.github/instructions/e2e.instructions.md`. They are the only intended copies; a human-facing guide
should link here instead.
