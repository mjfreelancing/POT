# Docker Setup for POT Project

This guide provides instructions for building, running, and managing the POT project's Dockerized services.

## Directory Structure and Key Files

- **Postgres Service**
  - Build context: `Postgres` directory
  - Dockerfile: `Postgres/Dockerfile`
- **Server Service**
  - Build context: project root (one level up from `Docker`)
  - Dockerfile: `Docker/Server/Dockerfile`
  - Entrypoint script: `Docker/Server/entrypoint.sh`
- **Solution File**
  - Located at the project root: `pot.sln`

## Ports

- The Dockerized server is exposed on port **5241**. Access the API at `http://localhost:5241/` when running via Docker Compose.
- The local development version of the server (when running directly with .NET tooling) uses port **5242**, as defined in the server's `launchSettings.json`.

## Environment Variables

### Docker Compose `.env` File

The `.env` file in the `Docker` directory defines variables for Docker Compose:

- `COMPOSE_PROJECT_NAME`: Prefix for Docker resources (containers, networks, volumes)
- `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`: Credentials and database name for the Postgres container and server connection

Docker Compose automatically loads these variables. They are referenced in `docker-compose.yml` using `${VAR_NAME}` syntax.

### Frontend (Vite/React) `.env` File

Frontend environment variables must be defined in a `.env` file at the root of the React project (`Source/Client/pot-react/`). Only variables prefixed with `VITE_` are available to the frontend code.

Example:

```
VITE_API_BASE_URL=http://localhost:5241/api
```

## Building and Running Docker Services

To build and start the services:

```bash
docker-compose up -d --build
```

To stop and remove the containers:

```bash
docker-compose down
```

To rebuild images after making changes to the source code:

```bash
docker-compose build
```

## Managing Docker Containers via Visual Studio Code

Predefined VS Code tasks are available for managing containers:

1. Open the Command Palette (`Shift+Ctrl+P` or `Shift+Cmd+P` on macOS)
2. Select `Run Task`
3. Choose one of the following:
   - `docker-start-pot`: Builds and starts the containers
   - `docker-stop-pot`: Stops and removes the containers

These tasks are defined in `.vscode/tasks.json`.
