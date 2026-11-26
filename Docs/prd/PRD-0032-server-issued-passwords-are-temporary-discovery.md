# PRD-0032 Server-issued passwords are temporary — Discovery

> Scratch document. Not a PRD and not a durable record. Delete this file once
> `PRD-0032-server-issued-passwords-are-temporary.md` exists and has been approved.

**Feature ID:** 0032
**Last Updated:** 2026-11-25
**Status:** Planning

## Captured input

Opened at the request of the discovery interview for PRD-0031, to be addressed at a later time. The instruction was to
consider **auto-declining an invitation after a period of time, or adding an `Expired` status** so an invitation
still appears in the interface — and, for the latter, to settle **what the user may then do with it**: re-invite,
delete, or something else.

The assessment that prompted it, recorded so this document stands on its own:

> The temporary password is generated server-side, mailed in plain text, and then stays a working credential
> indefinitely: the account is created `Pending` with that password already hashed, there is no expiry, no forced
> change, and nothing revokes it until somebody changes it by hand. The risk is therefore not that the password is
> mailed — every invitation carries some secret in mail — but that it never stops being valid, and that the address
> is typed by whoever raises the invitation. Two things bound it today: the approval gate keeps a `Pending` account
> out until an administrator approves it, and invitations are only raised by someone who may manage users.

Broadened at the owner's instruction after the survey behind PRD-0038: *"we need to look at other areas such as
signup and inviting users to see if they also need to force a password reset."* They do. The subject is no longer
one flow, so the title, the problem statement and the last sentence quoted above have all been corrected below.

## Correction to the recorded assessment

The quoted sentence **"the approval gate keeps a `Pending` account out until an administrator approves it"** is
false, and the risk it dismisses is therefore larger than the assessment concluded.

`AuthService.LoginAsync` at lines 156-161 reads:

```csharp
// Auto-enable invited users on first login
if (user.Status == UserStatus.Pending)
{
    user.Status = UserStatus.Enabled;
}
```

A `Pending` user is enabled and issued tokens. Only `Approval` is refused, at lines 163-166. An invitation creates
`Pending` (`InviteUserService.cs:91`), so an invited user signs in with the mailed password and is enabled by that
sign-in. The approval gate applies to self-sign-up (`VerifySignupService.cs:173`), which a platform administrator
approves through `UpdateApprovalService.cs:67-69`. Nothing gates an invitation.

An invitation's only remaining protections are that an administrator typed the address and that the mail arrived.

## Verified findings

- **Four flows make a server-generated string the stored password**, and none obliges the recipient to replace it.
  Reset: generated at `OtpService.cs:135`, hashed at 136, becomes the hash at
  `VerifyPasswordResetService.cs:84`. Sign-up: same generator, becomes the hash at `VerifySignupService.cs:174`.
  Invitation: generated and hashed in `InviteUserService.cs:83-84`. Re-send: regenerated in
  `ResendInviteService.cs:71-74`.
- **No forced-change mechanism exists anywhere.** Searching all of `Source` for `MustChangePassword`,
  `RequiresPasswordChange`, `PasswordChangeRequired` and `forcePasswordChange` returns no match, and
  `UserEntity` carries no such flag.
- **The plaintext password is in the mailbox, and in three templates on screen.** `SignupEmail.razor:62`,
  `InvitationEmail.razor:54` and `ChangePasswordEmail.razor:97` display it. `ChangePasswordEmail` closes with the
  advice "Update your password after sign in" at 110-115 — advice, not a requirement.
- **What differs between the four is who types the address.** Sign-up and reset are requested by the person who
  owns the mailbox. An invitation names an address typed by an administrator, which is the exposure the original
  assessment was describing.
- **One automatic invalidation already exists.** Re-sending an invitation replaces the stored hash, so the
  previously mailed password stops working. Nothing else retires a mailed password.
- **An invitation never expires.** There is no lifetime on the record, no `Expired` status (`UserStatus` offers
  `Enabled`, `Disabled`, `Pending` and `Approval`), and nothing in the interface distinguishes a live invitation
  from an abandoned one.
- **The password is never the only secret, but it is the durable one.** An OTP accompanies it in the reset and
  sign-up mail and is single-use; the password has no equivalent bound.
- **PRD-0035 referred a related option here.** Its Out Of Scope section records that hashing stored verification
  codes was judged of little value while the temporary password is stored only as a hash, "and suggested folding
  it into PRD-0032 if that reworks credentials". This document reworks credentials, so the referral becomes a
  question rather than a deferral — question 15.

## Draft problem statement

Draft, written to be corrected.

POT creates a password for a person, mails it in plain text, and then treats it as the password that person chose.
It happens on four paths — a password reset, a completed sign-up, an invitation and a re-sent invitation — and the
result is the same each time: the account's stored credential is a string that exists in a mailbox, with no expiry
and no obligation to change it.

The risk is not that the mail was intercepted in transit. It is that the password never stops being usable. Anyone
who later reads that mailbox — the recipient, a shared inbox, a forwarding rule, an archive, a backup, whoever
inherits the address — reads a working password, however long ago the mail arrived. Because nothing marks the
credential as temporary, its holder need never choose one of their own, and a system cannot tell the two apart.

Invitations sharpen this further. An administrator types the address by hand, so the password reaches a mailbox
that may belong to somebody else; the first sign-in enables the account, so no approval intervenes; and if the
invitation is never used it remains a working credential for as long as the record exists. Re-sending invalidates
the previous password but starts the same clock again.

The asymmetry is worth naming, because it is what makes this a defect rather than a preference. POT already treats
a signed-in password change as an event that ends every session (PRD-0038 will make a reset do the same). It does
not treat the moment a password is first handed out as anything at all.

## Scope sketch

**Appears in scope**

- Which flows hand out a server-generated password, and whether all four are subject to one rule or the invitation
  alone.
- The obligation itself: what records that a password was issued rather than chosen, where it is stored, when it is
  set and when it is cleared.
- What the server refuses while the obligation stands, and which routes are exempt so the person can still change
  the password, sign out or ask for help. Whether the refusal is enforced server-side rather than by the client
  redirecting to a form.
- Whether the reset flow becomes "set your own password now" at the point of verification, rather than mailing a
  password that must then be changed.
- What the person experiences: the step itself, what it blocks, and how it is reached on second and later sign-ins.
- What the three emails say now that the password is one-time and, where applicable, expires.
- Invitation expiry: after how long, whether the record moves to a visible `Expired` state, and what an
  administrator may then do with it — re-invite, delete, or something else.
- What happens to an expired invitation's password, and to a sign-in attempted with it.
- Notifications: what the invited person is sent on expiry, and what administrators see about stale invitations.
- How all of this interacts with the approval gate, with re-sending, and with the session revocation rule in
  PRD-0038.
- Whether the verification code encountered in PRD-0035 is hashed as part of this change (question 15).
- Automated coverage for the obligation and for expiry.

**Appears out of scope**

- Replacing the mailed password with a link or a single-use token — a related and probably better design, which
  this document does not decide. If it is chosen, questions 2 to 9 collapse.
- Password strength or composition rules beyond what exists today.
- Ending sessions when a password is reset or changed — PRD-0038, on which this document depends rather than
  duplicates.
- The E2E harness that reads invitation mail — PRD-0031.
- Account recovery for someone who ignores a mailed password for so long that it expires: they ask for another,
  which is the existing reset path.
- The verification-attempt limit that does not persist across requests, which PRD-0035 records as out of its own
  scope and is not adopted here.

## Clarifications

Settled before the rounds, so no question is spent on them:

- **The broadened scope is deliberate.** All four flows are in, because the rule is about the kind of password
  rather than the route that produced it; a per-flow answer would leave three of them as they are.
- **Invitation expiry stays in this document.** It is the same credential measured over time, and deciding the
  state and its actions separately would produce two documents describing one record.
- **A forced change needs a new `User` column**, and the owner will add and review the migration once the column is
  named here. Nothing is generated as part of writing this document.
- **This is not the same decision as revocation.** PRD-0038 closes a defect with no product choice in it; this
  document makes one.

## ROUND 1 — Functional requirements

> Answer inline after each `Answer:`. `unknown` is a valid answer, `n/a` drops the question, and
> `your call` accepts the recommendation. Add notes under `Other:` for a freeform answer.

### 1. Which flows must oblige the person to choose their own password?

- (a) All four: reset, sign-up, invitation and re-sent invitation — recommended, because the rule is about a
  password nobody chose, and each of the four produces one
- (b) Invitation and re-send only, which are the paths where an administrator types the address
- (c) Invitation, re-send and reset; sign-up is left as it is because the requester typed their own address

Answer:

### 2. What marks the password as temporary?

- (a) A `User` column, set whenever the server generates the password and cleared when the person sets their own —
  recommended, because nothing else distinguishes the two cases and the state must survive a restart
- (b) Infer it from the OTP record that produced it
- (c) Infer it from the account status

Answer:

### 3. What does the server refuse while the mark stands?

- (a) Everything authenticated except changing the password, signing out and reading the account's own details, all
  enforced on the server — recommended, because a client redirect is cosmetic
- (b) Only the routes that mutate or read data, allowing the rest
- (c) Nothing: the client shows the form and the server does not care

Answer:

### 4. Where does the person set the new password, and what is the current password then?

- (a) A dedicated step immediately after signing in with the mailed password, which asks only for the new password
  and its confirmation — recommended, because the mailed password is already known to them and asking for it again
  adds a way to fail
- (b) The existing change-password form, asking for the mailed password as the current one
- (c) A form reached from the reset page before any sign-in

Answer:

### 5. When is the mark cleared?

- (a) When the person successfully sets their own password, and by nothing else — recommended
- (b) Also when an administrator re-sends the invitation or resets the account
- (c) Also after a fixed period, whether or not they comply

Answer:

### 6. What happens if the mailed password expires before it is used?

- (a) It stops working, and the person is told to ask for a new one — recommended for invitations, which is the
  expiry this document decides
- (b) It keeps working; expiry only moves the record to a visible state
- (c) Unknown

Answer:

### 7. How long may an invitation stay live?

- (a) 14 days from issue, renewed by a re-send — recommended, because it is long enough for a holiday or a slow
  onboarding and short enough that an abandoned invitation is not a standing door
- (b) 7 days
- (c) 30 days

Answer:

### 8. What does an expired invitation look like to an administrator?

- (a) A distinct `Expired` state that stays in the interface, offering re-invite and delete — recommended, which is
  what the original input leaned toward
- (b) It disappears from the list, and only the audit trail shows it
- (c) It stays `Pending` and is told apart by its date

Answer:

### 9. What happens to the account when an invitation expires?

- (a) The user record stays, `Disabled`, and re-inviting re-enables and re-credentialises it — recommended, because
  the username and site association are already established and deleting them loses work
- (b) The user record is deleted
- (c) The record stays `Pending` and only the password is discarded

Answer:

### 10. Who is told, and when?

- (a) The invited person on expiry, and an administrator when an invitation has expired unused — recommended,
  because a silent expiry looks like a broken invite
- (b) Only the administrator, in the interface
- (c) Nobody

Answer:

### 11. What does a sign-in attempt with an expired or already-used mailed password say?

- (a) The same refusal as a wrong password, with the sign-in screen offering "forgot password" — recommended,
  because it reveals nothing to someone who did not receive the mail
- (b) A specific "your invitation expired" message
- (c) Unknown

Answer:

### 12. Should the reset flow ask for the new password instead of mailing one?

- (a) Yes: on verification, the person submits their own password, so no password is ever mailed on this path —
  recommended, because it removes the problem rather than marking it
- (b) No: keep mailing a temporary password and oblige a change at first sign-in
- (c) Keep mailing one for now and revisit after the first version

Answer:

### 13. What do the emails say once the password is one-time?

- (a) The password is described as temporary, with a single step to replace it and what happens if it is not used —
  recommended
- (b) The wording stays as it is
- (c) The password is removed from the email entirely (which assumes an answer to the out-of-scope question)

Answer:

### 14. Is the obligation visible to administrators?

- (a) Yes: a user whose password is still temporary is marked in the user list, so support can see why somebody is
  blocked — recommended
- (b) No: it is invisible outside the person's own session
- (c) Only in the audit trail

Answer:

### 15. Does this change hash the stored verification code, as PRD-0035 referred here?

- (a) No: keep it out, because it needs a migration, a new secret and constant-time comparison, and this document
  already carries one migration — recommended
- (b) Yes, include it, accepting that codes active at deploy time stop working
- (c) Unknown

Answer:

## Open Questions

None yet. Anything answered `unknown` moves here with enough context to stand on its own.
