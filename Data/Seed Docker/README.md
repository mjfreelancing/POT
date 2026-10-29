# POT Seed — PostgreSQL Container

This folder contains a Docker Compose setup that provides a local PostgreSQL database used to
**author the E2E seed** (`Source/Client/pot-react/e2e/seed/baseline.sql`).

The E2E suite does not use this container. Each run starts its own throwaway Testcontainers
database (port `55432`), runs the migrations, applies the seed file and destroys it afterwards, so
nothing here is needed to run the tests — only to change the seed they run against.

## Purpose

This container is a local development convenience, not a production service. It is intentionally lightweight and disposable — the container itself can be removed and recreated freely without losing anything that cannot be restored.

> Data is stored in `Data/Seed Docker/postgres-data/` on the host filesystem — it is a bind mount, not a Docker-managed volume. This means `docker compose down` (with or without `-v`) **never deletes your data**. To wipe the database, stop the container and manually delete the `postgres-data` folder.

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
.\Start-SeedPot.ps1
```

The script pulls the `postgres:13` image on first use, starts the container in the background, and prints the connection details and stop commands.

The container appears as the **pot-seed** group in Docker Desktop.

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
.\Start-SeedPot.ps1
```

The script recreates the `postgres-data` directory automatically before starting the container.

Then populate it using one of the two options below.

### Option A — Run migrations (empty database)

This creates the schema with no application data. Follow the steps in the
[Getting Started guide](../../Docs/GETTING-STARTED.md) to run migrations and seed initial data.

### Option B — Restore from a backup

The current dumps are in [`Backups/`](Backups/): `seed-dump-Pot-<timestamp>-baseline.tar` and
`…-financial.tar`. Each is a `pg_dump` archive of a freshly migrated database with the E2E seed
applied (the financial one also carries the accounts, expenses and incomes), so the archive brings
its own schema and can be restored into an empty `Pot` database:

```powershell
# Copy the archive in (PowerShell has no '<' input redirection), then restore it.
docker cp "Backups\seed-dump-Pot-<timestamp>-financial.tar" pot-seed-postgres:/tmp/dump.tar
docker exec pot-seed-postgres pg_restore -U postgres -d Pot --no-owner --clean --if-exists /tmp/dump.tar
```

Restore a dump of the same vintage as the application: an older archive carries an older schema and
identities the seed no longer has.
