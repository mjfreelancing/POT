# Configuration

What POT reads at startup, which file supplies each value, and which values must agree with one another.

Two files share the name `.env.development` and are unrelated: the Docker stack's is in `Source/Docker/`,
and the client build's is in `Source/Client/pot-react/`.

## The Docker stack

Two files feed the compose command, and both are passed explicitly:

```bash
cd Source/Docker
docker-compose --env-file .env --env-file .env.development -f docker-compose-client-server.yml up -d
```

| File               | Tracked | Supplied by                      | Holds                                                      |
| ------------------ | ------- | -------------------------------- | ---------------------------------------------------------- |
| `.env`             | yes     | nothing, it is in the repository | The compose project name and the database name             |
| `.env.development` | no      | you, copied from the template    | Credentials, JWT keys, SMTP, CORS origins, platform admins |

`Source/Docker/.env.development.template` is the tracked starting point and **the one copy of the key
list**. Copy it to `.env.development` in the same folder and edit your copy:

```powershell
cd Source/Docker
Copy-Item .env.development.template .env.development
```

The template's own comments explain each key. The groups below say what the API does with them and what
breaks when a value is wrong.

Omitting either `--env-file` leaves the `${...}` placeholders in the compose file empty, and the API then
fails its startup validation instead of starting with a missing value.

## The variable groups

| Group          | Keys                                                                                                                            | Constraint                                                                                                                                       |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Database       | `POSTGRES_USER`, `POSTGRES_PASSWORD`                                                                                            | The password must match the one the Postgres image is built with (`Source/Docker/Postgres/Dockerfile`, default `password`)                       |
| JWT            | `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET_KEY`                                                                                  | Issuer and audience must be the URL the API is reached at. The key must be exactly 128 characters, because the signing algorithm is HMACSHA512   |
| SMTP           | `SMTP_HOST`, `SMTP_PORT`, `SMTP_REQUIRE_TLS`, `SMTP_AUTH_USERNAME`, `SMTP_AUTH_PASSWORD`, `SMTP_FROM_NAME`, `SMTP_FROM_ADDRESS` | None may be blank. The template's placeholders satisfy validation only: sign-up, password reset, invitations and reminders need real credentials |
| CORS           | `CORS_ALLOWED_ORIGINS`                                                                                                          | Comma-separated absolute origins with no path, query, fragment or trailing slash. Must include the origin the client is served from              |
| Platform admin | `PLATFORM_ADMIN_USERIDS`                                                                                                        | Comma-separated user `RowId` values. May be empty until a user exists                                                                            |

## Values that must agree

| Value                        | Must match                                                                                 |
| ---------------------------- | ------------------------------------------------------------------------------------------ |
| `POSTGRES_PASSWORD`          | the Postgres image's password                                                              |
| `JWT_ISSUER`, `JWT_AUDIENCE` | the API URL - `http://localhost:5241` in Docker, `http://localhost:5242` when run directly |
| `JWT_SECRET_KEY`             | exactly 128 characters                                                                     |
| `CORS_ALLOWED_ORIGINS`       | the client URL - `http://localhost:5175`                                                   |
| `SMTP_*`                     | a provider account that accepts the settings                                               |

The API validates all of these during startup in every environment, so a wrong value stops the process
rather than surfacing at first use. The option groups it validates are listed under
[Configuration Validation](../../Source/Server/DEVELOPER.md#configuration-validation).

## The client build

`Source/Client/pot-react/.env.development` is read by `npm run dev`. It is git-ignored, so each developer
creates their own:

```bash
VITE_API_BASE_URL=/api
```

`/api` keeps the browser on the same origin, so the Vite proxy (`vite.config.ts`) forwards `/api` to the
API on port 5242 and the refresh cookie is same-site. The Azure image instead bakes an absolute API URL
at build time; see the client build arguments in the Docker guide.

## Running without Docker

The API reads `Pot.AspNetCore/appsettings.Development.json` when run with `dotnet run`, and the values
needed for a local start are listed in [Local setup](../how-to/run-locally-without-docker.md). The env file above is used only
by the compose stack.
