---
id: ADR-0026
title: Background work runs inside the API process
status: accepted
date: 2025-10-05
area: backend
---

# ADR-0026 (backfilled): Background work runs inside the API process

**Summary:** In the context of a small deployment with recurring work — reminder emails, expired code
cleanup, queued sends — facing the choice between a separate worker process and hosted background
workers inside the API, we decided to run the workers in the API process, to achieve one deployable and
no additional plumbing, accepting that the work stops when the API stops and that whenever the platform
scales out, the scheduled work is duplicated.

## Context and Problem Statement

The work runs outside requests: drain queued mail as it arrives, mark expired one-time codes as expired
every five minutes, and run the budget reminder cycle every hour. It needs the application's services and its database, and it needs
to run on a schedule.

A separate worker process would isolate the work and let it scale independently. It would also be a second
deployable, a second configuration surface, and a second thing that can be running an older version than
the API — for a deployment operated by one person.

The workers are kept in the API process for simplicity.

## Decision Drivers

- One deployable, one configuration, one version to keep current.
- The work depends on the same schema and services as the API, so it must start after migrations anyway.
- The volume is small and the schedule is coarse.

## Considered Options

1. Background workers hosted inside the API process.
2. A separate worker container with the same image and a different entrypoint.
3. An external scheduler invoking an endpoint.
4. A queue with a dedicated consumer service.

## Decision Outcome

Chosen option: 1. Each unit of background work is a hosted service in the API process: the two that touch
the database wait until it is ready before their first cycle and then run on a schedule, and the one that
drains the in-memory email channel simply starts draining when the host starts.

Every API replica hosts every worker, and nothing in the application coordinates them. A scheduled job
must therefore be correct when more than one process runs the same cycle, and when a process restarts
inside its period.

### Consequences

- Good, because there is one image, one deployment and one configuration, and the workers are always the
  same version as the API they share services with.
- Good, because a worker's dependency on the database is expressed once, through the readiness wait, rather
  than per job.
- Good, because nothing new was introduced to operate.
- Bad, because the work stops when the API stops and competes with requests for the same resources.
- Bad, because a second replica duplicates every scheduled cycle, and the deployment allows more than one
  ([ADR-0032](ADR-0032-production-runs-two-container-apps.md)), so horizontal scaling is a correctness
  risk rather than only a capacity setting. The risks to background work are listed under "Known
  scale-out risks" below.
- Bad, because a cycle runs on the same thread pool and database connection pool as requests, and is not
  observable except through logs.

### Confirmation

- Test: `BudgetReminderEmailWorkerFixture` and `ExpiredOtpCleanupWorkerFixture` assert that each waits for
  the `database` health check before creating the scope for its first cycle; the reminder fixture also
  asserts that one user's failure does not stop the rest of the cycle.
- Gap: `SendEmailWorker` and the channel it drains have no fixtures, no fixture asserts behaviour at host
  shutdown, and the hosted integration suite removes every hosted service, so no test runs a worker inside
  a host.
- Review: a new scheduled job must be written to be idempotent for its period, because the deployment
  can run more than one replica and nothing in the application prevents it. The budget reminder does not
  meet this today.

## Pros and Cons of the Options

### In-process (chosen)

- Good: nothing extra to deploy, and the workers match the API version exactly.
- Bad: scaling the API scales the workers with it, which is wrong for scheduled work.

### Separate worker container

- Good: independent scaling and lifecycle.
- Bad: a second deployable to version and configure, with its own start-up ordering against the same
  schema.

### External scheduler calling an endpoint

- Good: the schedule lives outside the application.
- Bad: adds an authenticated entry point that must be protected like a credential, and the work becomes
  request-shaped.

### Queue with a consumer

- Good: durable and scale-safe.
- Bad: infrastructure and delivery semantics that this stage of the product does not yet need.

## More Information

- `Docs/prd/PRD-0019-email-outbox-durable-dispatch.md` (Proposed) would make scheduled email safe under
  more than one replica and across restarts — a database outbox drained with claim and lease — while
  keeping the workers in the API process and adding no deployable, and it leaves the expired-code update
  out of scope. It would remove the reliance on one replica for email, not the in-process hosting; the
  record does not show that the hosting itself is expected to change.
- The email mechanism being queued in process is a separate decision with its own intended supersession.

### Known scale-out risks

These are the risks to background work. Each is accepted for now and owned by the PRD named, which
records it as a risk to resolve. They apply whenever more than one API replica runs, including a revision
overlap during a deployment. The replica range, and the scale-out risks that are not about background
work, are in [ADR-0032](ADR-0032-production-runs-two-container-apps.md).

- **Duplicate budget reminders.** Each replica runs the hourly cycle, so every admin gets one reminder per
  running replica. The reminder is not safe with one replica either: it records nothing, so a restart
  inside the trigger hour sends that hour's reminders again, and a trigger hour with no process running
  sends none. Owner: [PRD-0019](../prd/PRD-0019-email-outbox-durable-dispatch.md), which sets out
  both failures.

The expired-code update is idempotent, so a duplicate run of it is only redundant work. Sign-up, reset
and other transactional emails are not duplicated by scale-out: each replica drains only the queue in its
own memory.
