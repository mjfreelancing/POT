---
id: ADR-0030
title: Background work runs inside the API process under a single-replica assumption
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0030: Background work runs inside the API process under a single-replica assumption

**Summary:** In the context of a small deployment with recurring work — reminder emails, expired code
cleanup, queued sends — facing the choice between a separate worker process and hosted background
workers inside the API, we decided to run the workers in the API process and assume a single replica, to
achieve one deployable and no additional plumbing, accepting that the work stops when the API stops and
that scaling out would duplicate it.

## Context and Problem Statement

The work is scheduled and periodic rather than request-driven: send queued mail, sweep expired one-time
codes, run the budget reminder cycle. It needs the application's services and its database, and it needs
to run on a schedule.

A separate worker process would isolate the work and let it scale independently. It would also be a second
deployable, a second configuration surface, and a second thing that can be running an older version than
the API — for a deployment operated by one person.

## Decision Drivers

- One deployable, one configuration, one version to keep current.
- The work depends on the same schema and services as the API, so it must start after migrations anyway.
- The volume is small and the schedule is coarse.

## Considered Options

1. Background workers hosted inside the API process, with a single replica.
2. A separate worker container with the same image and a different entrypoint.
3. An external scheduler invoking an endpoint.
4. A queue with a dedicated consumer service.

## Decision Outcome

Chosen option: 1. Each unit of background work is a hosted service in the API process: the two that touch
the database wait until it is ready before their first cycle and then run on a schedule, and the one that
drains the in-memory email channel simply starts draining when the host starts. The deployment is assumed
to run one replica, so no two processes run the same cycle at the same time.

The single-replica assumption is what makes the scheduling safe: a per-process timer would otherwise run
the same job once per replica. It is an assumption about the deployment rather than something the
application configures, and nothing in the repository sets a replica count.

### Consequences

- Good, because there is one image, one deployment and one configuration, and the workers are always the
  same version as the API they share services with.
- Good, because a worker's dependency on the database is expressed once, through the readiness wait, rather
  than per job.
- Good, because nothing new was introduced to operate.
- Bad, because the work stops when the API stops and competes with requests for the same resources.
- Bad, because a second replica would duplicate every scheduled cycle, so horizontal scaling is currently
  blocked by the design rather than by capacity.
- Bad, because a long cycle holds a request thread pool slot and is not observable except through logs.

### Confirmation

- Test: `BudgetReminderEmailWorkerFixture` and `ExpiredOtpCleanupWorkerFixture` assert that each waits for
database readiness before its first cycle and does not run when the host is stopping.
- Gap: the email drainer has no fixture of its own; its behaviour is covered through the channel's tests.
- Review: a new scheduled job must be written to be idempotent for its period, because the single-replica
  assumption is environmental rather than enforced.

## Pros and Cons of the Options

### In-process, one replica (chosen)

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

- This record is expected to be superseded by a more scalable arrangement. The shape it would take is
  written up in `Docs/Future/PRD-019-email-outbox-durable-dispatch.md`: a database outbox drained with
  claim and lease, delivering email without a new deployable, and explicitly leaving the expired-code
  sweep out of scope.
- The email mechanism being queued in process is a separate decision with its own intended supersession.
