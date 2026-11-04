# Liveness and Readiness Probe Audit

Feature ID: 033
Status: Proposed
Date created: 2026-11-04
Scope: Audit only. This document records what is deployed and what is unclear, so that discovery can decide whether any further work is required. No code, configuration or deployment change is proposed here.
Audience: Future implementation agent(s), reviewers, and maintainers.

## Objective

Establish what the health probes on the deployed Container Apps actually do, and decide whether the
liveness/readiness split delivered in PRD-013 is wired to the platform in a way that matches its
intent.

## Problem To Solve

The application separates liveness from readiness: `/_health` reports liveness without touching the
database, and `/_health/ready` runs the checks tagged `ready`, including the `PotDbContext` check that
requires applied migrations. Background workers wait for readiness before their first cycle.

On the deployed Container Apps:

1. The API container app has a **liveness** probe configured for `/_health` on port 5241, with a
   30-second initial delay (to allow startup migrations), a 60-second period, a 5-second timeout and a
   failure threshold of 3.
2. **Readiness probes are enabled in the container app configuration but have no route configured.**
   It is not established what an enabled readiness probe with no path does — whether it is inert,
   defaults to some path, or reports the replica as not ready.
3. The client container app has a liveness probe on `/health` (served by nginx).

The deployment record in [Docs/Azure/Azure-Deployment-Notes.md](../Azure/Azure-Deployment-Notes.md)
was written against the live deployment and describes only liveness probes; the deployment is
click-ops, so there is no infrastructure-as-code in the repository that can be read instead.

The unresolved question is whether the current arrangement is equivalent to the intended design or
whether readiness is simply not applied to traffic routing. If the latter, HTTP traffic can reach a
replica whose database checks have not passed, and the split currently protects only in-process work.

## What Is Known

| Area | Fact | Source |
| --- | --- | --- |
| Liveness endpoint | `/_health`, no checks executed | `Pot.AspNetCore/Program.cs` |
| Readiness endpoint | `/_health/ready`, runs `ready`-tagged checks including the database check | `Pot.AspNetCore/Program.cs` |
| Worker gate | Both database-dependent workers wait for readiness before their first cycle | Worker startup gate and its unit tests |
| API container probe | Liveness on `/_health`, port 5241, 30 s initial delay, 60 s period, 5 s timeout, failure threshold 3 | Azure deployment record (owner confirms factual) |
| API container readiness | Enabled, no route configured | Owner, 2026-11-04 |
| Client container probe | Liveness on `/health` | Azure deployment record |
| Infrastructure as code | None; the deployment is configured by hand | Repository has no Azure IaC |

## Questions the Audit Must Answer

1. What does a readiness probe with no path configured actually do in the current Container Apps
   configuration — is it evaluated at all, and with what effective path?
2. Should the API readiness probe target `/_health/ready`, and what does that change: revision
   activation, replica traffic routing, or nothing observable at this scale?
3. Does the API's liveness probe on `/_health` remain correct, given a 30-second initial delay exists
   only to cover startup migrations?
4. Is the worker readiness gate still wanted if the platform gates traffic on readiness, and does
   anything else consume readiness?
5. Should the client container app also have a readiness probe, or is static file serving without one
   deliberate?
6. Does anything in the change need to be captured in the deployment record so the deployed state and
   the documentation stop differing?

## Out Of Scope

- Changing the behaviour or set of health checks until the audit concludes.
- Replacing `/_health` with a database-aware endpoint; liveness deliberately stays database-free.
- Introducing infrastructure as code for the deployment.

## Related Documents

- Feature index: [Docs/Future/README.md](README.md)
- DB readiness health check and worker startup gate: [PRD-013](PRD-013-db-readiness-health-check-and-worker-startup-gate.md)
- Deployed Azure configuration: [Docs/Azure/Azure-Deployment-Notes.md](../Azure/Azure-Deployment-Notes.md)
