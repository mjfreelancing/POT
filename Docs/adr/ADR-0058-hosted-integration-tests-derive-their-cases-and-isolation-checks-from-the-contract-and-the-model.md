---
id: ADR-0058
title: Hosted integration tests derive their cases and isolation checks from the contract and the model
status: accepted
date: 2026-10-28
area: backend
---

# ADR-0058: Hosted integration tests derive their cases and isolation checks from the contract and the model

**Summary:** In the context of a hosted test suite whose two safety properties are "every endpoint
authorises correctly" and "a destructive action touches only what it names", facing the cost of a hand-
written case per operation and a hand-kept list of tenant-scoped tables, we decided to derive both — the
authorization cases from the generated contract and endpoint metadata, the isolation scope from the EF
model — to achieve coverage that cannot rot when an endpoint or an entity is added, accepting a harness
that is itself code needing tests and a small, explicit list of operations it cannot judge.

## Context and Problem Statement

The API's risk does not sit in one endpoint. It sits in the invariants that hold for all of them: an
endpoint refuses a caller who lacks the permission it declares, a tenant's data is never reachable from
another tenant, and a destructive action removes what it names and nothing else. A test per operation
turns those invariants into hundreds of individually-maintained cases, and a test that lists the tables a
tenant owns turns the second into a list that is edited the day it breaks rather than the day it is
wrong.

Both sources of truth already exist in machine-readable form. The contract is generated at build time and
committed (ADR-0049), and a running host serves it through `DocumentedOperations`
(`Pipeline/Support/DocumentedOperations.cs`). The EF model knows which entities hold, or reach through
their foreign keys, a site's key (`Invariants/Support/TenantScopeResolver.cs`), and it knows every foreign
key (`Invariants/Support/OrphanSweep.cs`).

The harness built on those sources is in the tree: `AuthorizationMatrix` generates the cases,
`EndpointRequirements` reads each mapped route's `AllowAnonymous` marker and policies,
`InventoryCoverage` decides completeness, `IgnoredOperations` holds the exceptions, `TenantSnapshotter`,
`TenantFingerprint` and `GlobalEntities` prove isolation, and `CapturingEmailWriter` keeps the messages
the product sends so a test can read a credential without a mailbox. The behaviour is pinned by fixtures
that run the rules against operations the application does not contain (`InventoryCoverage`,
`MessageCaptureFixture`).

## Decision Drivers

- An endpoint added tomorrow must be exercised without anyone remembering to add a case for it.
- A list written by hand is the artifact that goes stale, and it fails silently when it does.
- A failure has to name what moved — the tenant, table, row and column — because a count hides it.
- The suite runs against a real pipeline, so it must stay affordable on a shared stack.

## Considered Options

1. A hand-written authorization case per operation, plus a hand-kept list of tenant-scoped tables.
2. Generate the cases from the contract and endpoint metadata, and resolve tenant scope from the model
   (status quo).
3. Exercise a sample of operations per resource by hand, and fingerprint a chosen subset of tables.

## Decision Outcome

Chosen option: 2, because it is the only option whose coverage is decided by the system rather than by
whoever remembered to write a case. The harness is not free: its rules are themselves tested on operations
the application does not contain, so the failure modes stay proven even while the real contract happens to
be fully covered.

The rules it imposes:

- The operation inventory is the generated contract, read from a running host. Never keep a list of
  operations beside the tests.
- The expected authorization outcome comes from the endpoint: its `AllowAnonymous` marker and policies,
  evaluated against a caller whose permissions are resolved through the application's own permission
  service. Never a table of intended outcomes.
- A generated case asserts the authorization decision, not a business outcome: 401 where anonymous callers
  are refused, 403 where the declared permission is missing, and neither where the declaration allows the
  caller.
- An operation may be ignored only through `IgnoredOperations`, with a reason a reviewer can judge — it
  ends the session, tenant or identity the run depends on, its answer is decided by something other than
  the caller's authorization, or the only body it accepts would change data. "Not written yet" is not a
  reason.
- Tenant scope is resolved from the model, never listed. An entity that reaches no site fails until it is
  declared in `GlobalEntities` with a reason, and an entry that has since become tenant-scoped fails too.
- Isolation is proven by fingerprinting the tenant the action must not see and the tenant it acts in,
  before and after, and reporting the first difference.
- The orphan sweep runs the model's own foreign keys after every destructive action; an action can report
  success and still leave a reference behind.
- The harness never gives the running product a way to record a message. The capture lives in the test
  project, and `MessageCaptureFixture` asserts the shipped assembly offers exactly one writer.

### Consequences

- Good, because an endpoint or an entity added later is covered without anyone remembering: the contract
  and the model are the inventory.
- Good, because a failure names the tenant, table, row and column, so a broken invariant reads as itself.
- Good, because the suite asserts what the endpoint declared rather than what someone believed it should
  do, so a wiring mistake and a test mistake are different failures.
- Bad, because the harness is code: it needs its own tests, and its rules are the only way to know the
  coverage claim is real.
- Bad, because an operation whose only bindable body would change data cannot be judged by a case and has
  to be ignored, so the exception list is not empty and must stay visible.
- Bad, because the inventory comes from the OpenAPI document, so an operation missing from the contract
  would be missing from the matrix too.
- Bad, because fingerprinting whole tenants is slow, which is why it is applied to destructive actions
  rather than to every test.
- Bad, because a developer has to learn the harness — matrix, coverage, snapshotter, scope resolver, orphan
  sweep — before a failing case reads as a diagnosis.

### Confirmation

- Test: `InventoryCoverage` and the invariant rules are exercised against operations and entities the
  application does not contain, so the failure modes stay proven independently of the real contract.
- Test: `MessageCaptureFixture` asserts the shipped assembly offers exactly one writer and that the capture
  lives in the test project.
- Instruction: `.github/instructions/aspnetcore-integration-tests.instructions.md`.
- Review: a hand-written list of operations, a table of intended authorization outcomes, or a hand-kept
  list of tenant-scoped tables is a defect, not a shortcut.

## More Information

- The rules the matrix checks come from ADR-0047 (every endpoint is authenticated unless explicitly
  anonymous) and ADR-0021 (permissions resolved per request).
- Tenant confinement in the product is structural (ADR-0030); the fingerprinting harness is how a
  destructive action is proven to respect it.
- Scenario provisioning through the product's own routes is a separate decision, ADR-0051.
- The contract the inventory reads is generated and committed (ADR-0049).
