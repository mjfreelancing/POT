# PRD-032 Invitation credential lifecycle — Discovery

> Scratch document. Not a PRD and not a durable record. Delete this file once
> `PRD-032-invitation-credential-lifecycle.md` exists and has been approved.

**Feature ID:** 032
**Last Updated:** 2026-10-27
**Status:** Planning

## Captured input

Opened at the request of the discovery interview for PRD-031, to be addressed at a later time. The instruction was to
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

Verified facts about the current state:

- An invited user is created with `Status = Pending` and the temporary password's hash. The plaintext password
  exists only in the invitation message.
- No expiry, no attempt counter and no forced password change attach to an invitation.
- The invitation can be re-sent through the existing resend route, which generates a new temporary password.
- `UserStatus` currently offers `Enabled`, `Disabled`, `Pending` and `Approval`. There is no expired state.
- The approval gate is the only thing standing between a leaked invitation password and access.

## Draft problem statement

Draft, written to be corrected.

An invitation creates a working credential with no lifetime. The password is generated, mailed, and remains valid
until the invited person changes it — which nothing obliges them to do, and which cannot happen at all if they never
sign in. An invitation that is ignored for months is therefore an open door whose address is known to whoever reads
that mailbox, and typed by hand by whoever raised it.

Because the account sits `Pending`, the approval gate limits immediate damage: an approval is still required before
the credential is useful. But that leaves the lifecycle incoherent — the invitation itself never expires, while the
approval it waits for is a separate act that may never come. Nothing in the interface tells an administrator which
invitations are still live, so an abandoned one is indistinguishable from a fresh one.

## Scope sketch

**Appears in scope**

- What happens to an invitation over time: whether it expires, after how long, and what state the record moves to.
- Whether an expired invitation remains visible in the interface, and what actions are offered on it — re-invite,
  delete, or something else.
- How the invited person experiences an expired invitation if they follow the message late.
- What happens to the credentials of an invitation that expired: whether the password stops working, and how.
- The notification, if any, sent when an invitation expires.
- How the change interacts with the approval gate and with re-sending an invitation.

**Appears out of scope**

- Replacing the temporary password with a link or token — a related but separate decision, and the reason this
  document is not that change.
- The automated test harness that reads invitation messages — specified by PRD-031, which this document does not
  depend on.

## Clarifications

Filled in during Stage 2. One question at a time in chat; answers summarised here so the rounds can build on them.
Anything answered `unknown` moves to Open Questions.

## ROUND 1 — Functional requirements

> Answer inline after each `Answer:`. `unknown` is a valid answer, `n/a` drops the question, and
> `your call` accepts the recommendation. Add notes under `Other:` for a freeform answer.

### 1. <question>

- (a) <option> — recommended, because <reason>
- (b) <option>
- (c) <option>

Answer:

## Open Questions

- **Auto-decline or a visible expired state.** Whether an invitation simply stops being usable, or moves to a state
  that is shown in the interface. The captured input leans toward the second, which then requires the next item.
- **What an administrator may do with an expired invitation.** Re-invite, delete, both, or something else — and
  whether those actions differ from the ones already offered on a pending invitation.
- **How long an invitation stays live.** Not yet discussed; needs a default and the reasoning behind it.
