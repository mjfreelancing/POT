# Auth and sessions

Lookup facts for the authentication mechanism. How the flows fit together is in
[Authentication](../explanation/authentication.md).

## Endpoints

All under `/api/auth`, all anonymous.

| Method | Path                     | Purpose                                            |
| ------ | ------------------------ | -------------------------------------------------- |
| POST   | `/signup/send`           | Validate the details and email a verification code |
| POST   | `/signup/complete`       | Submit the code and create the account             |
| POST   | `/login`                 | Verify the password and start a session            |
| POST   | `/refresh`               | Exchange the refresh cookie for a new access token |
| POST   | `/logout`                | Revoke the current session and clear the cookie    |
| POST   | `/password-reset/send`   | Email a reset code                                 |
| POST   | `/password-reset/verify` | Submit the code and the new password               |

## The access token

| Property        | Value                                                                |
| --------------- | -------------------------------------------------------------------- |
| Format          | JWT, signed with HMACSHA512                                          |
| Lifetime        | 15 minutes                                                           |
| Signing key     | `Jwt:SecretKey`, exactly 128 characters, validated at startup        |
| Issuer/audience | `Jwt:Issuer`, `Jwt:Audience` - must be the URL the API is reached at |

Claims, and nothing else:

| Claim           | Content                                                                   |
| --------------- | ------------------------------------------------------------------------- |
| `jti`           | A new GUID per token                                                      |
| `sub`           | The user's `RowId`                                                        |
| `token_version` | The user's token version at issue time (`PotClaimTypes.TokenVersion`)     |
| `session_id`    | The `AuthSession.RowId` this token belongs to (`PotClaimTypes.SessionId`) |

`MapInboundClaims` is disabled, so `sub` stays `sub` rather than being rewritten to
`ClaimTypes.NameIdentifier`.

## The refresh cookie

| Property | Value                                                                                    |
| -------- | ---------------------------------------------------------------------------------------- |
| Name     | `pot_refresh_token`                                                                      |
| HttpOnly | Yes - browser script cannot read it                                                      |
| Secure   | `Authentication:Cookie:SecureOnly`, `true` by default; `false` only for local HTTP       |
| SameSite | `Lax` - sent on top-level navigation and page reload, not on cross-site subrequests      |
| Path     | `/`                                                                                      |
| Lifetime | `Authentication:Cookie:MaxAgeDays`, 30 days by default                                   |
| Domain   | `Authentication:Cookie:Domain`; empty for same-origin, `.example.com` to span subdomains |

The cookie is the only place the refresh token exists in the browser. The server never stores the token
itself, only its SHA-256 hash.

Clearing the cookie has to use the same `Domain` it was set with, or the browser removes a host-only
cookie and the domain-scoped one survives until it expires.

## The session row

One `AuthSession` row per sign-in (`Pot.Data/Entities/AuthSessionEntity.cs`).

| Column                   | Purpose                                                                   |
| ------------------------ | ------------------------------------------------------------------------- |
| `RowId`                  | Assigned by the caller so the access token can carry it before the insert |
| `UserId`                 | The signed-in user                                                        |
| `RefreshTokenHash`       | SHA-256 of the refresh token, hex encoded, unique index                   |
| `CreatedUtc`             | When the session began                                                    |
| `ExpiresUtc`             | When the refresh token stops being accepted                               |
| `RevokedUtc`             | Set on sign-out or on a password change                                   |
| `LastSeenUtc`            | Updated on each refresh                                                   |
| `UserAgent`, `IpAddress` | Recorded for support, truncated to the column limits                      |

A session is usable only while `RevokedUtc` is null and `ExpiresUtc` is in the future.

## What revokes what

| Action               | Effect                                                                                          |
| -------------------- | ----------------------------------------------------------------------------------------------- |
| Sign out             | That session's `RevokedUtc` is set and the cookie is cleared. Other devices keep their sessions |
| Refresh              | The hash is replaced and the expiry extended - the previous refresh token stops working         |
| Change password      | Every session for the user is revoked, and `TokenVersion` is incremented                        |
| Disable the account  | Sign-in is refused; the account is rejected when its token is validated on the next request     |
| Access token expires | Nothing to revoke - the client refreshes                                                        |

`TokenVersion` and sessions are separate controls. A session row answers "is this device still signed
in"; the token version answers "is every token issued before this moment still valid". A password change
needs both, because a token issued seconds earlier is still valid on its own.

## Permissions

- Format is `resource:action`, for example `account:view` or `user:manage`, enumerated in `Permission`.
- Effective permissions are the union of the caller's role permissions and the deployment-configured
  platform grant, resolved from the database on every request.
- `platform:manage` is the one permission that comes from the platform grant rather than a role.
- A fail-closed fallback policy makes any endpoint that declares no requirement of its own authenticated
  only; the routes that must stay reachable while signed out are marked `AllowAnonymous()` explicitly, and
  a test enumerates the host's endpoints and fails if a route is neither.

## Account status

| Status     | Meaning                                                      | Can sign in |
| ---------- | ------------------------------------------------------------ | ----------- |
| `Pending`  | Invited, invitation not yet accepted                         | No          |
| `Approval` | Self-service sign-up verified its email, awaiting a decision | No          |
| `Enabled`  | Approved and active                                          | Yes         |
| `Disabled` | Rejected, or disabled by an administrator                    | No          |

## Storing the token in the browser

The access token lives in memory only. The comparison that drove that choice:

| Storage          | Script readable | Survives reload | XSS exposure                        |
| ---------------- | --------------- | --------------- | ----------------------------------- |
| `localStorage`   | Yes             | Yes             | Full token theft                    |
| `sessionStorage` | Yes             | Per tab         | Full token theft                    |
| Memory           | Yes             | No              | Only while the page is open         |
| HttpOnly cookie  | No              | Yes             | Not readable; CSRF becomes the risk |

POT uses memory for the access token and an HttpOnly cookie for the refresh token, so a script cannot
read the long-lived credential. The cookie is `SameSite`-scoped and the API accepts credentialed
cross-origin requests only from configured origins.
