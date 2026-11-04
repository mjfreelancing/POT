---
id: ADR-0018
title: The OpenAPI document is generated at build time and committed
status: accepted
date: 2026-11-04
area: backend
---

# ADR-0018: The OpenAPI document is generated at build time and committed

**Summary:** In the context of an API whose contract exists only as the code that declares it, facing
the need for a reviewable description of what each endpoint accepts, returns and fails with, we decided
to generate the OpenAPI document during the build and commit it, to achieve a contract that is diffed
with the change that alters it, accepting that the committed copy must be regenerated and reviewed
rather than being derived on demand.

## Context and Problem Statement

Endpoint declarations already carry their metadata: the request and response models, the response types,
and the failures each route can produce. That metadata is only visible by reading the endpoint
declarations or by running the application.

A document generated at runtime cannot be reviewed in a pull request, and an unpublished document
cannot be compared across changes. Generating it during the build and committing it makes the contract
an artefact of the change rather than of the running process.

## Decision Drivers

- A change to a request or response shape should be visible as a diff in the same change.
- The document should describe the declared failures, not only the happy path.
- Generating it must not become a manual step that can be skipped.

## Considered Options

1. Generate at build time and commit the document.
2. Generate at runtime and serve it, without committing anything.
3. Hand-write the contract document.
4. Generate at runtime only, and never publish it.

## Decision Outcome

Chosen option: 1. The build writes the document for the current version into the repository, and a
release build overrides the runtime-only default so the file is always produced from the same code the
binary is built from. Each endpoint declares the failures its own rules can produce, so the document's
failure sets match behaviour rather than listing a maximal set.

The document is not currently consumed by the client: client API types are hand-written. It is committed
because it is the clearest description of the contract that exists, and because a future consumer — a
generator, or an assistant-facing server — can be built on it without the API having to change.

### Consequences

- Good, because a shape change produces a diff in the same commit, and a forgotten regeneration is
  visible as an unexpected document change.
- Good, because the failure set per endpoint is stated once, where the endpoint is declared.
- Good, because a consumer that is not the React client has a machine-readable contract to start from.
- Bad, because the committed file is a generated artefact in version control: it can conflict, and a
  careless merge can leave it inconsistent with the code.
- Bad, because the declared failures must be maintained deliberately; the generator records what is
  declared, not what actually happens.
- Bad, because nothing currently verifies that the client code matches the document, so the two can
  drift silently.

### Confirmation

- Test: `EndpointResponseMetadataFixture` pins which failures each endpoint declares, which is what the
  document records.
- Review: a change to a request or response shape that does not change the document is a missing
  regeneration, not a no-op.
- Known gap: no check compares the committed document to a freshly generated one; detecting a stale copy
  relies on the reviewer.

## Pros and Cons of the Options

### Build-time, committed (chosen)

- Good: reviewable, diffable, and independent of the process being run.
- Bad: an artefact in the repository that must be kept fresh.

### Runtime only, served

- Good: always current by construction.
- Bad: cannot be reviewed before deployment, and comparing two versions needs both running.

### Hand-written

- Good: complete freedom, including prose.
- Bad: drifts from the code immediately, and duplicates metadata the endpoints already declare.

### Runtime only, unpublished

- Good: nothing to maintain.
- Bad: no contract at all for anyone outside the code.

## More Information

- The client types being hand-written rather than generated is a separate, current state.
- Per-endpoint failure declarations are also what keeps the client able to read any body it receives.
