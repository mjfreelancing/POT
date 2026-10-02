# Docker

- Prefer VS Code tasks `docker-start-client-server` / `docker-stop-client-server` over ad-hoc shell sequences.
- Primary compose file: `Source/Docker/docker-compose-client-server.yml`. Direct compose commands must load `--env-file .env --env-file .env.development`.
- Containers: `pot-react`, `pot-aspnet`, `pot-postgres`. Host ports: client `5175`, API `5241`, Postgres `5444`.
- Health: API `http://localhost:5241/_health`, client `http://localhost:5175/health`.
- After any lifecycle change, verify the containers are running and both health endpoints respond. On failure, report the root error and the smallest next corrective step.
- Treat `Source/Docker/postgres-data/**` as protected runtime data: never modify it unless explicitly asked.
- Keep Dockerfile/compose edits minimal and limited to the requested behavior; preserve existing `network`, `depends_on` and `healthcheck` intent unless asked otherwise.
