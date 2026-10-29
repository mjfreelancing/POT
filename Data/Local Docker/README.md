# POT Local — PostgreSQL Container (scratch database)

This folder contains a Docker Compose setup that provides the **scratch database**: restore a dump into
it and inspect the result, or point the API at it while working. It is not the database the API uses by
default — `appsettings.Development.json` points at the prodlike stack on `5444`.

## Purpose

This container is a local development convenience, not a production service. It is intentionally lightweight and disposable — the container itself can be removed and recreated freely without losing anything that cannot be restored. Nothing lives only here: the prodlike stack and the seed-authoring database each have their own backups, and a dump can be restored into this one again.

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

### Option B — Restore from a dump

`Backups/` holds this container's own dumps, `local-dump-Pot-<yyyyMMddHHmm>.tar`. The prodlike stack's
dumps are in `Data/ProdLike Backups/` and the seed-authoring database's in `Data/Seed Docker/Backups/`,
and all of them restore the same way. A `pg_dump` archive carries its own schema, so it can go into an
empty database:

```powershell
# Copy the archive in (PowerShell has no '<' input redirection), then restore it.
docker cp "Backups\local-dump-Pot-<timestamp>.tar" pot-local-postgres:/tmp/dump.tar
docker exec pot-local-postgres pg_restore -U postgres -d Pot --no-owner --clean --if-exists /tmp/dump.tar
```

`--clean --if-exists` replaces what the database holds, and `--no-owner` makes the restore portable
between machines. If the current contents matter, back this container up first.

Restore a dump of the same vintage as the application: an older archive carries an older schema.
