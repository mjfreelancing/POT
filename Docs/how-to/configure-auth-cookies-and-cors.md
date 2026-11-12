# Configure auth cookies and CORS

The refresh token travels in an HTTP-only cookie, so the cookie settings and the API's allowed origins
have to agree with the address the browser uses. A mismatch shows up as a sign-in that works once and
then fails on reload.

The mechanism is in [Auth and sessions](../reference/auth-and-sessions.md); this page is the
configuration and the symptoms.

## The cookie settings

| Setting                            | Default | Use                                                                                                     |
| ---------------------------------- | ------- | ------------------------------------------------------------------------------------------------------- |
| `Authentication:Cookie:SecureOnly` | `true`  | `false` only where the API is reached over plain HTTP and the browser does not treat the host as secure |
| `Authentication:Cookie:MaxAgeDays` | `30`    | How long the refresh token is accepted, which is also the session lifetime                              |
| `Authentication:Cookie:Domain`     | empty   | Empty for a same-origin deployment; `.example.com` to share the cookie across subdomains                |

In the Docker stack these are environment variables, so the double-underscore form applies:
`Authentication__Cookie__SecureOnly`, `Authentication__Cookie__MaxAgeDays`,
`Authentication__Cookie__Domain`.

**Local HTTP works with `SecureOnly` left at `true`** because browsers treat `localhost` as a secure
context. The E2E harness sets `--Authentication:Cookie:SecureOnly=false` so the API behaves the same for
`127.0.0.1`, which is not treated as secure in every browser.

If you set a `Domain`, the cookie is set for that domain only. Clearing it has to use the same domain,
which the sign-out path does - otherwise the browser removes a host-only cookie and the real one survives
until it expires.

## CORS origins

`CORS_ALLOWED_ORIGINS` is a comma-separated list of absolute origins, and it must include the origin the
browser loads the client from.

- Include the scheme and the port: `http://localhost:5175`, not `localhost:5175`.
- No path, query, fragment or trailing slash.
- `*` is rejected. Credentialed requests cannot use a wildcard origin, and the refresh cookie is a
  credentialed request.
- An invalid entry fails startup validation rather than failing at request time.

## Symptoms and what they mean

| Symptom                                                         | Likely cause                                                                                                                      |
| --------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Sign-in succeeds, the first reload signs the user out           | The refresh cookie was not stored: wrong origin in CORS, or `SecureOnly` true over an origin the browser does not treat as secure |
| The browser reports a CORS error on `/api/auth/refresh`         | The client origin is missing from `CORS_ALLOWED_ORIGINS`, or it has a trailing slash or path                                      |
| The cookie is visible in the browser but not sent               | `SameSite=Lax` with a cross-site call, or a `Domain` that does not cover the API host                                             |
| Sign-out appears to do nothing, and the user is still signed in | The cookie was set with a `Domain` and cleared without one, so the browser kept it                                                |
| A `401` that never recovers                                     | The session was revoked or expired; the client sends the user to sign in after a failed refresh                                   |

Two checks settle most of these quickly:

1. In the browser's application storage, confirm a `pot_refresh_token` cookie exists for the API's host,
   and note its `Secure`, `SameSite`, `Domain` and expiry.
2. Call the refresh endpoint directly with the cookie and see whether it answers with a new token or a
   `401`.

## Change the settings

1. Edit the env file or application setting for your environment - see
   [Configuration](../reference/configuration.md) for where each value lives.
2. Restart the API. Cookie options and CORS policy are built at startup.
3. Sign in again; existing sessions are unaffected, but a cookie with the old settings stays until it
   expires, so clear it when you change `Domain` or `SecureOnly`.

## Related

- [Auth and sessions](../reference/auth-and-sessions.md) - endpoints, claims and session rows
- [Ports and environments](../reference/ports-and-environments.md) - the origin your client is served from
- [Configure platform administrators](configure-platform-admins.md) - the platform grant
