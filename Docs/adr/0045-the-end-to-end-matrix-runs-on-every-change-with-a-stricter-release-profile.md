---
id: ADR-0045
title: The end-to-end matrix runs on every change, with a stricter profile before release
status: accepted
date: 2026-11-04
area: frontend
---

# ADR-0045: The end-to-end matrix runs on every change, with a stricter profile before release

**Summary:** In the context of a browser suite that is slow enough to be run deliberately and valuable
enough not to skip, facing the choice between one configuration and two, we decided on a development
profile covering four browser engines that runs for any client-affecting change, and a production-like
profile that runs against a production build on demand and before a release, to achieve confidence
without paying the strictest cost every time, accepting that the stricter profile is a discipline rather
than a gate.

## Context and Problem Statement

Two things can go wrong that only a browser shows: the interface behaves differently in one engine, and it
behaves differently when built for production rather than served by the development server. The first is
cheap to cover with the same stack; the second needs a built client and a different base URL.

Running the production-like profile on every change would double the slowest gate. Running only the
development profile risks a build-only failure — a base URL, an asset path, a service worker — reaching a
deployment.

## Decision Drivers

- Engine-specific behaviour should be caught before a release, not after.
- The everyday suite must finish in a time someone will accept.
- The production build differs from the development server in ways that matter.

## Considered Options

1. A development matrix by default plus a production-like profile on demand and before a release.
2. One configuration only, always.
3. Both profiles on every change.
4. No fixed policy; run whatever is convenient.

## Decision Outcome

Chosen option: 1. The default profile runs the suite across four browser engines against the API and the
development client, with fixed per-test timeouts and reporters configured in the project rather than on the
command line. The production-like profile builds the client, serves it the way a deployment would and runs
the suite against it; it is run on demand and before a release, and for any change that affects the client.

### Consequences

- Good, because an engine-specific failure is found by the default run rather than by a user.
- Good, because the strict profile exists and is specified, so "before release" is a command rather than an
  intention.
- Good, because timeouts and reporters live in the project, so two people running the suite get the same
  behaviour.
- Bad, because nothing enforces the production-like run; a release can be made without it.
- Bad, because four engines multiply the default suite's duration by four.
- Bad, because the default profile never exercises the built client, so a build-only defect is found by the
  profile that is easiest to skip.

### Confirmation

- Review: a client-affecting change states that the production-like profile was run, or why not.
- Review: a test that needs a longer timeout changes the project's configuration deliberately rather than
  passing a flag.

## Pros and Cons of the Options

### Two profiles (chosen)

- Good: everyday cost is bounded; the strict run is available and specified.
- Bad: the strict run depends on a person.

### One configuration

- Good: one thing to run and one to reason about.
- Bad: either the everyday run is slow or the production build is never tested.

### Both on every change

- Good: no discipline required.
- Bad: the slowest gate becomes slow twice over, and it will start being skipped.

### No policy

- Good: total freedom.
- Bad: the suite's coverage becomes whatever the last person ran, and no one can say what was tested.

## More Information

- The shared stack and the test classification are separate decisions.
- Continuous integration does not currently run the suite; that is a separate decision.
