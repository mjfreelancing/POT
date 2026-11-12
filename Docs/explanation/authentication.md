# Authentication

How someone gets into POT and stays signed in. The lookup facts - endpoints, token claims, cookie
settings, session columns - are in [auth and sessions](../reference/auth-and-sessions.md).

## One credential in memory, one in a cookie

A signed-in browser holds two things:

- an **access token**, kept only in memory in the client, sent as a bearer token on every API call and
  valid for 15 minutes;
- a **refresh token**, held in an HTTP-only cookie the JavaScript cannot read, and backed by a row in
  `AuthSession`.

The access token is never persisted, so a page reload loses it. The client then calls the refresh
endpoint, the cookie travels automatically, and a new access token comes back. That is why a reload does
not sign the user out, and why a stolen access token is only good for quarter of an hour.

## Sign-up and verification

Sign-up is two requests, because the address has to be proved before an account exists:

1. **Send.** The caller submits a username, email and password. The API validates them, stores a one-time
   code and emails it. Nothing is created yet.
2. **Complete.** The caller submits the username and the code. A matching, unexpired code creates the
   user, hashes the password with ASP.NET Core's `PasswordHasher`, and sets the account's status to
   `Approval`.

A code is single use, expires after 15 minutes, and accepts at most three attempts before it is
invalidated. Repeated requests are rate limited.

## Approval

A new account cannot sign in until a platform administrator approves it. Approval is a decision recorded
on the user: approving sets the status to `Enabled`, rejecting sets it to `Disabled`. Only
`platform:manage` may approve or reject. Self-service sign-up therefore never produces a usable account
on its own - someone with the platform grant has to act.

An invited user follows the same flow with one difference: the invitation creates the account in
`Pending` status, and accepting the invitation moves it on.

## Signing in

A successful sign-in does three things in one transaction:

1. verifies the password against the stored hash;
2. inserts an `AuthSession` row for this sign-in, recording the refresh token's SHA-256 hash, the
   expiry, and the caller's user agent and IP address for support;
3. returns an access token carrying that session's id, and sets the refresh cookie.

Only an `Enabled` account can complete this. An account awaiting approval gets its own response - a `200`
whose status is `Approval` and which carries a message - so the sign-in page can say "waiting for
approval" instead of "wrong password". Any other refusal, a disabled account included, is a problem
response, the same shape as bad credentials.

## Staying signed in

The client refreshes **before** the access token expires, on a timer, rather than waiting for a `401`.
Each refresh rotates the refresh token: the session row's hash is replaced and its expiry extended.
Because the stored value is a hash, a copy of the database cannot be used to mint sessions.

If a refresh arrives after the session was revoked or has expired, it fails and the client sends the user
to the sign-in page.

## Signing out

Sign-out revokes the session row and clears the cookie. Every access token issued to that session is
rejected on its next request, because each request resolves the session from the database rather than
trusting the token's lifetime. Other devices stay signed in - they have their own rows.

## Changing or resetting a password

Both paths end the same way: the password is re-hashed and **every** session for the user is revoked.
Changing a password also increments the user's token version, which invalidates access tokens already
issued even for sessions that were not explicitly revoked.

A forgotten password is reset with the same one-time-code mechanism as sign-up: request a code, receive
it by email, then submit the code with the new password. A signed-in user changing a password supplies
the current one instead.

## Why the token carries no derived state

The access token holds only four claims: `jti`, `sub`, `token_version` and `session_id`. Tenancy,
permissions and account status are read from the database on every request.

A JWT payload is readable by anyone holding the token, so putting a role or a site in it would leak that
information and, worse, go stale while the token stayed valid. Resolving from the database means
disabling a user, changing a role or moving a session takes effect on the next request, not after fifteen
minutes.

## Where the pieces live

| Piece                                                      | Location                                                      |
| ---------------------------------------------------------- | ------------------------------------------------------------- |
| Sign-in, refresh, sign-out, sign-up, password reset routes | `Pot.AspNetCore/Features/Auth`                                |
| Access token creation                                      | `Pot.AspNetCore/Concerns/Auth/Services/JwtService.cs`         |
| Session rows, rotation, revocation                         | `Pot.AspNetCore/Concerns/Auth/Services/AuthSessionService.cs` |
| One-time codes                                             | `Pot.App/Features/Otp`                                        |
| Approval                                                   | `Pot.App/Features/Approvals`                                  |
| Permission resolution                                      | `Pot.AspNetCore/Concerns/Auth` and `Pot.App/Features/Auth`    |
