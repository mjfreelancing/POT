# PRD-0038 Password reset ends existing sessions — Discovery

> Scratch document. Not a PRD and not a durable record. Delete this file once
> `PRD-0038-password-reset-ends-existing-sessions.md` exists and has been approved.

**Feature ID:** 0038
**Last Updated:** 2026-11-25
**Status:** Planning

## Captured input

Raised as item **F-3** of the temporary backlog:

> **F-3 · A completed password reset leaves every existing session valid** — *PRD.*
> - `AuthService.cs:311` is the only caller of `RevokeAllSessionsForUserAsync`, and it sits in the
>   signed-in password-change path. The reset-completion path revokes nothing and does not increment
>   `User.TokenVersion`, so a stolen session survives the reset the user performed to kill it.
> - **Cost of deferring:** the one action a user takes in response to a suspected compromise does not
>   disconnect the attacker.

The instruction that shaped the scope: *"we need to look at other areas such as signup and inviting
users to see if they also need to force a password reset."* That survey found four flows that make a
server-generated password the account's stored credential. The obligation to replace such a password —
and the expiry of an unused invitation — is a separate rule and is being decided in PRD-0032. This
document covers only the rule that a change to a password has to end the sessions that predate it.

## Verified findings

- **Two doors, one intent, different outcomes.** A signed-in user changing their password goes through
  `AuthService.ChangePasswordAsync`, which revokes every session at line 311, increments
  `User.TokenVersion` at line 314 and commits once at line 316. A signed-out user who resets a forgotten
  password lands in `VerifyPasswordResetService.VerifyResetAsync`, which assigns
  `user.PasswordHash = otp.TempPasswordHash!` and saves. It revokes nothing and increments nothing.
- **No new password is ever submitted.** `POST /password-reset/verify` takes `Username`,
  `ReferenceCode` and `VerificationCode` and nothing else. The password that becomes the account's
  credential was generated at `OtpService.cs:135`, hashed at 136, and emailed in plain text. This is
  PRD-0032's subject; it is recorded here only because it means the reset path has no "new password"
  that the user chose.
- **A stolen refresh token survives indefinitely.** `AuthService.cs:330` sets the refresh expiry to
  `now.AddDays(RefreshTokenExpiryDays)`, and `AuthSessionService.RotateRefreshToken` (line 78) rewrites
  `ExpiresUtc` to a fresh value on every use. The window therefore slides and never closes, so
  revocation is the only thing that can end a session — an expiry will not do it.
- **`RevokeAllSessionsForUserAsync` has exactly one caller.** `AuthSessionService.cs:90` sets
  `RevokedUtc` on every unrevoked session of the user and returns a count without saving; the caller
  commits. `ChangePasswordAsync` is the only caller in `Source/Server`.
- **A live access token is not the problem; the refresh token is.** Access tokens live 15 minutes
  (`JwtService.cs:14`) and are already rejected when their embedded version is stale, which is what the
  `TokenVersion` increment in `ChangePasswordAsync` is for.
- **`Pot.App` cannot reach the session service.** `VerifyPasswordResetService` lives in `Pot.App`;
  `IAuthSessionService` lives in `Pot.AspNetCore`. The dependency runs the other way, which is the whole
  of the implementation difficulty and the reason question 6 exists.
- **Two documents state the opposite of the code.** `Docs/explanation/authentication.md:73` says every
  session is revoked on both paths, and `Docs/adr/ADR-0036-...md:75` records as a *Bad* consequence that
  the reset path "touches neither the version nor the sessions". The second is accurate about the code
  and was written by an agent during a documentation re-organisation, so it records an observation
  rather than a decision: no Considered Option in ADR-0036 weighs revoking on reset.
- **`Docs/reference/auth-and-sessions.md` was wrong independently of this feature, and is now
  corrected.** Line 18 described `/password-reset/verify` as taking a new password, which it does not;
  it now names the username and the verification code. The "What revokes what" table at lines 76-84
  still has no password-reset row, which is an omission to fill once question 1 is answered.

## Draft problem statement

Draft, written to be corrected.

A user resets their password because they believe somebody else may have access to their account. It is
the one action available to them in response to a suspected compromise, and the one action whose value
depends entirely on it ending access that already exists. It does not. The reset replaces the password,
saves, and returns success; every session created before the reset stays valid, and its refresh token
keeps sliding forward in 30-day instalments for as long as somebody uses it.

The account already knows how to do this correctly. The signed-in change-password path revokes every
session and increments the version that makes already-issued access tokens useless. So the same intent,
reached through a different route, has opposite effects — and the route a worried user is most likely to
take, because they may no longer trust the device they are signed in on, is the route that does nothing.

## Scope sketch

**Appears in scope**

- What a completed password reset must end: every auth session for the user, and the version increment
  that invalidates access tokens already issued.
- Whether the rule is stated once, for any change to a stored password, so that a third path cannot be
  added later without inheriting it — rather than listed per route, which is how the gap arose.
- Whether the reset request itself, or any other credential-establishing administrative action, is
  subject to the same rule.
- What the user is told, in the interface and in the reset confirmation email, about other devices
  having been signed out.
- Where the revocation seam lives, given that the reset service and the session service are in
  different projects and the dependency cannot be reversed.
- The documents that become wrong or that stop being accurate: the ADR-0036 consequence bullet, the
  explanation page, the revocation table in the reference page, and the `AuthService` header comment
  that enumerates the global-revoke paths. The reference page's description of the reset request is
  already corrected, because it described something the endpoint never did.
- Automated coverage: a reset that completes must leave no refreshable session behind. No such test
  exists today.

**Appears out of scope**

- Obliging the user to choose their own password after a reset, or after an invitation or sign-up, and
  the expiry of an unused invitation — PRD-0032, which owns the whole lifecycle of a server-issued
  password.
- The OTP attempt counter that is never persisted on failure, so verification can be retried without
  limit — a separate backlog item.
- Logout leaving a session active when it cannot resolve the refresh token, and a replayed rotated
  token not revoking its session — a separate backlog item.
- A session-management screen listing and ending individual devices — no such surface exists and this
  document does not create one.
- The absence of an absolute lifetime on refresh tokens — a real weakness, but a distinct decision about
  how long a session may live rather than about what ends one.

## Clarifications

Settled before the rounds by the scope decision, so no question is spent on them:

- **This is not a policy choice.** The reset path is inconsistent with the change-password path it
  duplicates, and no document argues for the inconsistency. It is treated as a defect to close.
- **It is a separate PRD from PRD-0032.** Revocation needs no product decision, no schema change and no
  client surface; forcing a change of password needs all three. Bundling them would hold the security
  fix behind the larger discussion.
- **The migration question does not arise here.** Nothing in this feature needs a new column, so none is
  requested. If an answer to the rounds implies one, that will be raised before it is written.

Nothing else was unclear enough to change what the rounds ask.

## ROUND 1 — Functional requirements

> Answer inline after each `Answer:`. `unknown` is a valid answer, `n/a` drops the question, and
> `your call` accepts the recommendation. Add notes under `Other:` for a freeform answer.

### 1. What must a completed reset end?

- (a) Every session for the user, plus a `TokenVersion` increment — recommended, because it is what
  `ChangePasswordAsync` already does and it is the only thing that stops a refresh token that has no
  absolute lifetime
- (b) Every session, without the version increment — leaves live access tokens usable for up to 15
  minutes, which is the gap the version exists to close
- (c) Only sessions on other devices, sparing the current one — the reset is anonymous, so there is no
  current session to spare

Answer:

### 2. Is the rule written once or per route?

- (a) Once: any write to the stored password ends every session and increments the version, enforced at
  a single seam so a future path cannot omit it — recommended, because listing routes is exactly how
  the reset path came to omit it
- (b) Per route: add the revocation to the reset path and leave `ChangePasswordAsync` as it is
- (c) Once, but as a rule stated in a document that reviewers must apply to new code — no enforcement

Answer:

### 3. Does an administrative password change fall under the same rule?

- (a) Yes: any service that replaces a stored password ends that user's sessions — recommended, because
  otherwise the rule holds only for the paths that happen to exist today
- (b) No: only the two user-facing paths
- (c) Only if and when such an administrative path is added

Answer:

### 4. What does the person whose session was ended see?

- (a) The ordinary rejection, with a message on the sign-in screen saying the password was changed, so
  they know why they were signed out — recommended, because a silent ejection is indistinguishable from
  a fault and the person may be the attacker wondering what happened
- (b) The ordinary rejection with no explanation
- (c) Nothing special; whatever the client already does on a failed refresh

Answer:

### 5. Is the message different for the person who performed the reset?

- (a) No: the reset is anonymous, so a completed reset has no signed-in person to notify; the
  confirmation tells them other devices were signed out — recommended
- (b) The reset confirmation says nothing about sessions
- (c) The confirmation lists the devices that were ended

Answer:

### 6. Where does the revocation live, given the projects?

- (a) An abstraction in `Pot.App` describing "end every session for this user", implemented beside the
  existing session service in `Pot.AspNetCore` — recommended, because it keeps reset logic where it is
  and gives both credential paths one seam
- (b) Move reset completion out of `Pot.App` into the auth service layer in `Pot.AspNetCore`
- (c) Have the app service work on `IAuthSessionRepository` directly

Answer:

### 7. Does the rule reach the reset request as well as its completion?

- (a) Only on completion: the password does not change until the code is verified — recommended
- (b) Also when a reset is requested, ending sessions before the code is entered
- (c) On both, but only the second is treated as a security event

Answer:

### 8. What is recorded when sessions are ended?

- (a) An information-level log naming the user and the number of sessions revoked, as
  `ChangePasswordAsync` does — recommended, because the count is the only signal that the rule ran
- (b) Nothing
- (c) A full audit record of each session ended, kept for later inspection

Answer:

### 9. What is the automated proof?

- (a) A test that signs in, resets the password, and asserts the earlier refresh token is refused and
  that a token issued before the reset is rejected — recommended
- (b) A test that only asserts `RevokeAllSessionsForUserAsync` was called
- (c) No new test; existing coverage is judged sufficient

Answer:

### 10. Are there flows that deliberately keep a session alive across a password change?

- (a) None known; if one exists it should be named before the rule is enforced — recommended
- (b) The current device should keep working after the user sets a new password
- (c) Unknown

Answer:

## Open Questions

None yet. Anything answered `unknown` moves here with enough context to stand on its own.
