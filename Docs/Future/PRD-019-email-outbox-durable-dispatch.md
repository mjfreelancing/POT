# Email Outbox and Durable Dispatch PRD

**Status:** Proposed (discovery draft)
**Priority:** High
**Last Updated:** 2026-09-23
**Feature ID:** 019
**Scope:** Server-side email dispatch only — a durable, idempotent outbox for scheduled notifications, the worker that drains it, and the changes to `BudgetReminderEmailWorker` required to make it scale-safe. No client changes. No Azure Functions or other new deployables. The periodic OTP cleanup worker is explicitly out of scope.
**Audience:** Future implementation agent(s), reviewers, and maintainers.
**Depends on:** [PRD 018 — Site Locale and Time Zone Settings](PRD-018-site-locale-settings.md) (defines the site-local occurrence date used as the deduplication key).

## Purpose

Make scheduled email dispatch — currently only the budget reminder — correct when the API runs as more than one replica, and durable when a replica restarts or is replaced mid-cycle, without introducing a distributed lock, a second deployable, or a message broker.

## Problem Statement

`BudgetReminderEmailWorker` is registered as a hosted service (`AddOtpCleanup()` in `Pot.AspNetCore/Features/Auth/Extensions/ServiceCollectionExtensions.cs`) and runs in **every** replica. Its loop ticks hourly and, when the site's configured `LocalHourTrigger` matches the current local hour, iterates every enabled admin user and calls `IBudgetReminderService.SendRemindersAsync`. There is no coordination between replicas and no record of what has already been sent.

Consequently:

1. **Duplicate sends under horizontal scale.** With N replicas, each user receives N budget reminders. The deployment target is Azure Container Apps, which can run multiple replicas and can activate a new revision alongside the old one during a deploy.
2. **Duplicate sends on restart, even with a single replica.** The loop evaluates the current hour immediately on start and then hourly, so any restart landing inside the trigger hour re-sends. A restart at 06:05 and another at 06:40 produce two emails for the same day.
3. **Missed sends with no recovery.** Matching is strict hour equality (`if (reminderSettings.LocalHourTrigger != currentLocalHour) return;`). If no process is running during the trigger hour — scale-to-zero, cold start, a deploy spanning the window — that day's reminder is lost. There is no catch-up.
4. **DST edge cases.** With the zone fixed to `Australia/Sydney` (see PRD-018), a trigger hour inside a transition window can fire twice (fall-back repeats the local hour) or never (spring-forward skips it).

The dispatch path itself (`Pot.EmailSender`) has further defects that are only exposed once the above is fixed, because they are properties of the in-process transport rather than of the scheduling:

5. **The transport is not durable.** `SendEmailChannel` is a singleton wrapping `Channel.CreateUnbounded<EmailChannelConfig>()`. Anything queued when the process exits — a deploy, a crash, a scale-in — is lost, including reminders already accepted by the API. There is no retry.
6. **A transient SMTP failure silently drops the message.** `ProcessEmailsAsync` reads an item from the channel and only then calls the sender; the exception is caught by the loop's `catch` and the item is already gone.
7. **The channel's concurrency contract is violated.** `SingleWriter = true` is declared, but `ISendEmailChannelWriter.SubmitAsync` is called from scoped services during HTTP requests (`RequestPasswordResetService`, `RequestSignupService`, `VerifySignupService`, `InviteUserService`, `ResendInviteService`, `UpdateApprovalService`, `BudgetReminderService`), i.e. concurrently from multiple request threads.
8. **No backpressure.** The channel is unbounded; a slow or unreachable SMTP server grows the queue in memory with no limit.

Diagnostic that bounds the scope: **`ExpiredOtpCleanupWorker` does not need this treatment.** It performs a single idempotent update of expired OTP rows, so duplicate execution across replicas is redundant work rather than a correctness defect. It is deliberately excluded.

## Current Behaviour

### The scheduler

- `Pot.AspNetCore/Features/Workers/BudgetReminderEmailWorker.cs` — gates on the `"database"` health check via `IServiceHealthPoller`, then loops hourly. Each iteration resolves users from the outer scope, creates a **per-user scope** to set `ICurrentUserContext.UserRowId`, and calls `IBudgetReminderService.SendRemindersAsync`. `nextUtc` is computed as `now.AddHours(1)` truncated to the hour.
- `Pot.App/Features/Notifications/BudgetReminder/BudgetReminderService.cs` — returns early when reminders are disabled or the configured local hour does not match; otherwise builds `EmailBudgetReminderInfo` and calls `ISendEmailChannelWriter.SubmitAsync`.
- Users are enumerated across all sites by `GetAllUsersService.GetAllEnabledAdminsAsync`. This relies on `UserEntity` having no global query filter (only `Account`, `AccountAccrual`, `Expense`, `Income` and `Setting` are filtered in `PotDbContext.SetupQueryFilters`).

### The transport

- `Pot.EmailSender/SendEmailChannel.cs` — implements both reader and writer over one unbounded `Channel<EmailChannelConfig>`; registered as a singleton, with the writer resolved by casting the reader.
- `Pot.AspNetCore/Features/Workers/SendEmailWorker.cs` — drains the channel.
- `Pot.RazorComponents/Models/EmailType.cs` — `Signup`, `ChangePassword`, `Invitation`, `PendingApproval`, `ApprovalAccepted`, `ApprovalRejected`, `BudgetReminder`.
- `Pot.EmailSender/IEmailSender.cs` / `EmailSender.cs` — one send method per type, dispatched through a `Func<EmailConfigBase, CancellationToken, Task>` switch. Rendering (Razor + plain text) and the MailKit `SmtpClient` connection are per message.
- `Pot.RazorComponents/Models/EmailConfigBase.cs` — `Username` and `Email` are the recipient fields carried on every payload.

### Persistence conventions

- `Pot.Data/Entities/EntityBase.cs` — `Id`, `RowId`, `Etag`; `Pot.Data/DbContextBase.cs` derives the table name from the entity type (`<Name>Entity` → `<Name>`), disables cascade delete, and sets `QueryTrackingBehavior.NoTrackingWithIdentityResolution` by default.
- `Pot.Data/Migrations` + `PotDbMigrator` — migrations run before the API serves traffic, and `/_health/ready` only reports healthy once at least one migration is applied.
- `Pot.AspNetCore.Integration.Tests/Host/ApiWebApplicationFactory.cs` removes hosted services (`services.RemoveAll<IHostedService>()`), so integration tests exercise services directly rather than the workers.

## Proposed Solution

Candidate requirements; nothing is implemented. Shape: **email-only outbox** (see Design Notes for the rejected alternatives).

- **R1 — `EmailOutbox` entity and migration.** A table recording one row per message to dispatch, with at minimum: the payload discriminator (`EmailType`), the recipient snapshot (`Username`, `Email`), a lifecycle `Status`, an attempt count, a lease holder and expiry, the created/next-attempt timestamps, and a nullable **occurrence** key. Follows the existing `EntityBase`/migration conventions.
- **R2 — Occurrence key and unique index.** For scheduled notifications, the occurrence is the **site-local date** derived under PRD-018 (for example `2026-09-23`), and the unique key is `(TargetUserId, EmailType, OccurrenceDate)`. The index is filtered to rows where the occurrence is not null, so non-scheduled messages are unconstrained. This key is what makes concurrent enqueues idempotent.
- **R3 — Idempotent enqueue.** Enqueueing a scheduled notification is an insert that tolerates collision (`ON CONFLICT DO NOTHING`); a losing caller does nothing. Enqueueing stops being a decision that requires exclusivity.
- **R4 — Claim-and-lease drainer.** A worker claims due rows in a bounded batch using `SELECT … FOR UPDATE SKIP LOCKED` (raw SQL — no EF Core LINQ equivalent), stamping a lease holder and expiry in the same statement, then sends outside the transaction, then marks the row `Sent` or reschedules it.
- **R5 — At-least-once with bounded retry.** A failed send increments the attempt count and sets `NextAttemptUtc` with backoff; after a configured maximum the row moves to a terminal `Failed` state rather than being discarded. A crashed drainer's rows are reclaimable once the lease expires.
- **R6 — Same-day catch-up.** A pending occurrence whose site-local day has not yet ended is still eligible to send; once the site-local day has passed it is abandoned rather than sent late.
- **R7 — Recipient snapshot, no request context.** The row carries the recipient details needed to send, so the drainer runs without an `ICurrentUserContext` and the table carries no site query filter. This avoids coupling an infrastructure worker to the per-request site resolution.
- **R8 — Replace the in-process channel.** Scheduled notifications are enqueued to the outbox instead of the channel; the drainer replaces `SendEmailWorker`. Whether the remaining (transactional) message types also move is OD-03.
- **R9 — Preserve the enqueue seam.** Keep the existing `ISendEmailChannelWriter`-style call site contract so the seven writer services do not each need reworking, and so the change is reviewable as a transport swap rather than a cross-cutting rename. See OD-04.
- **R10 — Retention.** Define retention for `Sent` and `Failed` rows and implement it with the same pattern chosen for AuthSession retention in PRD-010, so there is one cleanup idiom rather than two. See OD-05.
- **R11 — Observability.** Log per-cycle enqueued/sent/failed counts and expose the count of pending and failed rows, so "it did not run" and "it is stuck" are distinguishable.

## Non-Goals

- **No distributed lock or leader election.** The design removes the need for mutual exclusion rather than supplying it.
- **No new deployable.** Azure Functions and Container Apps Jobs are out of scope; see Design Notes.
- **No Azure Service Bus / broker integration.** Postgres is already a dependency and provides the required claim semantics.
- **No changes to OTP cleanup.** `ExpiredOtpCleanupWorker` stays as it is; duplicate execution is harmless.
- **No generic job-scheduling framework.** The outbox covers message dispatch; it is not a general scheduler for arbitrary periodic work.
- **No email template or rendering changes.**

## Tests

Planned coverage, following the existing split between unit and integration tests:

- **Entity and migration** — the filtered unique index exists and enforces the occurrence key; `Pot.Data.Tests` conventions for repository fixtures.
- **Idempotent enqueue** — two enqueues with the same key produce one row; a re-run after a restart produces no additional row.
- **Claim exclusivity** — two concurrent drainers over the same batch claim disjoint rows; the total sent count equals the row count.
- **Lease recovery** — a row claimed with an expired lease is reclaimed; a row claimed with a live lease is not.
- **Retry and terminal failure** — a throwing sender reschedules with backoff and eventually reaches `Failed` without data loss.
- **Catch-up window** — a pending occurrence inside the site-local day is sent; one outside it is abandoned.
- **Scheduler behaviour** — the reminder scheduler enqueues once per user per occurrence regardless of how many times the cycle runs. Extends `Pot.AspNetCore.Tests/Features/Workers/BudgetReminderEmailWorkerFixture.cs`.
- **Integration** — enqueue/send contract through `ApiWebApplicationFactory`. Note that hosted services are removed there, so the drainer is exercised directly or through a dedicated fixture rather than via the default factory.

## Open Decisions Backlog

| ID    | Decision                                                                        | Candidate Options                                                                                                                     | Status  |
| ----- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| OD-01 | How the payload is stored                                                       | Serialise `EmailConfigBase` to JSON with the `EmailType` as discriminator · Store identifiers and rebuild at send time                | Pending |
| OD-02 | Drain cadence and latency mechanism                                             | Short fixed poll interval · Poll plus `LISTEN`/`NOTIFY` wake-up · Long interval (scheduled messages only)                             | Pending |
| OD-03 | Which message types use the outbox                                              | Scheduled only (transactional types stay on the in-process channel) · All types (one transport, channel deleted)                      | Pending |
| OD-04 | How the enqueue seam is shaped                                                  | Keep `ISendEmailChannelWriter` and reimplement it against the outbox · Introduce `IEmailOutboxWriter` and update all seven call sites | Pending |
| OD-05 | Retention for `Sent` / `Failed` rows                                            | Delete after N days · Keep indefinitely · Align with the PRD-010 cleanup worker                                                       | Pending |
| OD-06 | Lease duration, batch size, backoff schedule and maximum attempts               | Values to be tuned; must be expressible as configuration or documented constants                                                      | Pending |
| OD-07 | Whether `Etag` optimistic concurrency is used for claim/update, or raw SQL only | Raw SQL claim + status update · EF Core tracked update guarded by concurrency token                                                   | Pending |
| OD-08 | Whether the outbox is exposed to operators through an existing surface          | Not exposed · Counts surfaced through logging/metrics only · New endpoint                                                             | Pending |

## Outstanding Questions Register

1. Is at-least-once acceptable for every email type if OD-03 moves transactional messages — specifically, can an invitation or approval notification be duplicated by a retry, or must those remain exactly-once-in-practice?
2. Does a duplicate budget reminder carry any real cost beyond annoyance given the target usage (fewer than five users)? This determines how much of OD-06 needs tuning versus defaulting.
3. Where should the site-local occurrence date be computed — in the scheduler before enqueue, or inside the enqueue path — and does the answer change when PRD-018 lands?
4. Should the reminder scheduler keep its own hourly cadence once enqueue is idempotent, or move to a shorter interval to reduce the worst-case delivery latency to the top of the hour?
5. Does the outbox need to record which replica sent a message for operational diagnosis, or is the lease holder sufficient?
6. Should a `Failed` row raise a signal (log level, metric, alert) automatically, or is it inspected on demand?

## Assumptions and Constraints Register

| ID    | Assumption / Constraint                                                                                                                                                     | Confidence |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| AC-01 | Postgres is a hard dependency of the API, so `FOR UPDATE SKIP LOCKED` is always available and no new infrastructure is required.                                            | High       |
| AC-02 | The API already gates workers on the `"database"` health check, so the drainer can reuse `IServiceHealthPoller` unchanged.                                                  | High       |
| AC-03 | Only the budget reminder is currently schedule-driven, so exactly one message type needs an occurrence key today.                                                           | High       |
| AC-04 | Container Apps may run more than one replica and may overlap revisions during a deploy; duplicate execution must be correct, not merely unlikely.                           | High       |
| AC-05 | Raw SQL is required for the claim (EF Core has no LINQ construct for `SKIP LOCKED`), and the default no-tracking query behaviour means claim/update paths must be explicit. | High       |
| AC-06 | PRD-018 delivers the site-local date; until then the occurrence key would have to fall back to the fixed Sydney zone.                                                       | Medium     |

## Discovery Exit Criteria

Before this PRD moves from Proposed to Planning:

1. OD-01 is decided, because it determines the schema and the drainer's dispatch path.
2. OD-03 is decided, because it determines whether the in-process channel and `SendEmailWorker` are deleted or retained.
3. OD-02 is decided, because it is the only requirement that can introduce user-visible latency for transactional mail.
4. PRD-018 has at least decided OD-03 (zone modelling) there, so the occurrence key is well defined.
5. The retention decision (OD-05) is consistent with PRD-010, so the repository does not end up with two cleanup idioms.

## Design Notes (Rationale and Rejected Alternatives)

Reference notes for future maintainers. Rationale recorded here would otherwise be lost when this PRD moves to implementation.

- **Why the outbox removes the need for a lock rather than adding one.** The duplicate arose from every replica independently _deciding_ to send. Splitting "decide" (insert with a unique key) from "do" (claim with `SKIP LOCKED`) means concurrent deciders collide harmlessly and concurrent doers take disjoint work. Scaling replicas then increases throughput instead of duplication, which is the opposite property to a leader-election design.
- **Rejected: Postgres advisory lock or a lease table as the primary fix.** A lock makes only one replica _run_, so it fixes scale-out duplication but not the restart-inside-the-trigger-hour duplicate (the remainder of the cycle still sends), and it leaves the send at-least-once with no record. It also introduces stale-holder handling for no benefit over the idempotency key. Recorded as a fallback only.
- **Rejected: a generic scheduled-job runner (schedule table + job handlers).** It would also absorb `ExpiredOtpCleanupWorker` and the PRD-010 purge, and would give per-job run history. It was rejected for this increment because neither of those workers is incorrect under duplication (one is a single idempotent `UPDATE`, the other is a delete), so the framework would add a scheduler, a handler abstraction, a second table and a second worker to solve a problem that only the email path actually has. Revisit if a second schedule-driven job genuinely needs the claim/lease primitive; extracting it from two real call sites is preferred over designing it for one hypothetical one.
- **Rejected: Azure Functions timer trigger.** The dispatch path terminates in an in-process channel, so a Functions host would enqueue into _its own_ singleton channel that the API's `SendEmailWorker` never drains; it would need a duplicate email stack or an internal endpoint back into the API. It would also need the database access, health gate and site/user context plumbing, and would still require the idempotency key for at-least-once delivery — a second deployable for no reduction in work. Transactional email also depends on the in-process channel in a way that a split host would break.
- **Rejected: pinning `max-replicas=1` as the fix.** It removes the scale-out duplicate but not the restart duplicate, and it caps throughput as a permanent constraint. It remains a valid _interim_ operational guard alongside this work.
- **`FOR UPDATE SKIP LOCKED` rather than a status-only claim.** A two-step "select pending, then update to claimed" allows two drainers to select the same rows. Skipping locked rows makes claim atomic per row without a coordinating component.
- **A lease rather than a single long transaction.** Holding a transaction open across an SMTP connection would hold a database connection and lock rows for the duration of network I/O. Claim → send → mark, with an expiring lease, keeps transactions short and makes a crashed drainer recoverable.
- **The recipient is snapshotted onto the row.** Resolving the recipient during dispatch would require a current-user/site context in an infrastructure worker and would couple the outbox to the site query filters. Carrying `Username`/`Email` on the row keeps the drainer context-free (R7).
- **Occurrence key is a date, not a timestamp or an hour.** An hour-based or timestamp-based key reproduces the DST defects; a site-local date is stable across a DST transition and makes same-day catch-up a natural property rather than a special case.
- **This is a correctness fix, not a throughput feature.** At the current scale the duplicate volume is trivially small; the reason to do the work is that scaling the API out — a normal, intended operation — silently breaks a user-facing promise.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Prerequisite: [Site Locale and Time Zone Settings](PRD-018-site-locale-settings.md)
- Worker startup gate that the drainer reuses: [DB Readiness Health Check and Worker Startup Gate](PRD-013-db-readiness-health-check-and-worker-startup-gate.md)
- Retention idiom to align with: [AuthSession Retention and Background Cleanup Policy](PRD-010-auth-session-retention-and-cleanup-policy.md)
- Affected code: `Source/Server/Pot.AspNetCore/Features/Workers`, `Source/Server/Pot.EmailSender`, `Source/Server/Pot.App/Features/Notifications/BudgetReminder`
