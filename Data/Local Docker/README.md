# Local POT — PostgreSQL Container

This folder contains a Docker Compose setup that provides a local PostgreSQL database for use **while running the POT .NET API in Visual Studio** (or via `dotnet run`).

## Purpose

This container is a local development convenience, not a production service. It is intentionally lightweight and disposable — the container itself can be removed and recreated freely without losing anything that cannot be restored.

> Data is stored in `Data/Local Docker/postgres-data/` on the host filesystem — it is a bind mount, not a Docker-managed volume. This means `docker compose down` (with or without `-v`) **never deletes your data**. To wipe the database, stop the container and manually delete the `postgres-data` folder.

## Connection details

| Setting  | Value       |
| -------- | ----------- |
| Host     | `localhost` |
| Port     | `5432`      |
| Database | `Pot`       |
| Username | `postgres`  |
| Password | `password`  |

## Starting the container

Run the PowerShell script from this folder:

```powershell
.\Start-LocalPot.ps1
```

The script pulls the `postgres:13` image on first use, starts the container in the background, and prints the connection details and stop commands.

The container appears as the **pot-local** group in Docker Desktop.

## Stopping the container

```powershell
# Stop the container (data folder untouched)
docker compose -f "docker-compose.yml" stop

# Stop and remove the container (data folder untouched)
docker compose -f "docker-compose.yml" down
```

## Starting fresh

If you need a clean database, stop the container, delete the `postgres-data` folder, then start again:

```powershell
docker compose -f "docker-compose.yml" down
Remove-Item -Recurse -Force .\postgres-data
.\Start-LocalPot.ps1
```

The script recreates the `postgres-data` directory automatically before starting the container.

Then populate it using one of the two options below.

### Option A — Run migrations (empty database)

This creates the schema with no application data. Follow the steps in the
[Getting Started guide](../../Docs/GETTING-STARTED.md) to run migrations and seed initial data.

### Option B — Restore from a backup

If you have a previous export, you can restore it manually using `pg_restore` or
`psql` after running migrations to create the schema:

```powershell
# Example using psql (adjust the file path and credentials as needed)
psql -h localhost -p 5432 -U postgres -d Pot -f "path\to\backup.sql"
```

Previous exports are stored in [`Data/Exports/`](../Exports/).
