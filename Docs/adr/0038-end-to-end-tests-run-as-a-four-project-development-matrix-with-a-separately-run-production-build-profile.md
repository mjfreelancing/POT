---
id: ADR-0038
title: End-to-end tests run as a four-project development matrix, with a separately run production-build profile
status: accepted
date: 2026-05-11
area: cross-cutting
related-prds: [PRD-0012]
supersedes: [ADR-0040]
---

# ADR-0038 (backfilled): End-to-end tests run as a four-project development matrix, with a separately run production-build profile

**Summary:** In the context of a browser suite that is slow enough to be run deliberately and valuable
enough not to skip, facing the choice between one configuration and two, we decided on a development
profile running the suite across four projects — two Chromium variants, a mobile Chromium device and a
mobile WebKit device — and a production-like profile that runs the built client against the API, gating the
completion of every PRD that changes the client, to achieve confidence without paying the strictest cost
every time, accepting that neither profile runs automatically, so the gate is kept by convention.

## Context and Problem Statement

Two things can go wrong that only a browser shows: the interface behaves differently in one engine, and it
behaves differently when built for production rather than served by the development server. The first is
cheap to cover with the same stack; the second needs a built client and a different base URL.

Running the production-like profile on every change would double the slowest gate. Running only the
development profile risks a build-only failure — a base URL, an asset path, a service worker — reaching a
deployment.

When the production-like profile must run was written down two ways. The delivery skills and
`e2e.instructions.md` made it the completion gate for a PRD that changed client product code. `e2e/README.md`
and ADR-0040 called it on demand, when the production build changes and before releases. There are no
formal releases, so "before releases" named no trigger.

## Decision Drivers

- Engine-specific behaviour should be caught before a release, not after.
- The everyday suite must finish in a time someone will accept.
- The production build differs from the development server in ways that matter.

## Considered Options

1. A development matrix by default plus a production-like profile that is run separately.
2. One configuration only, always.
3. Both profiles on every change.
4. No fixed policy; run whatever is convenient.

## Decision Outcome

Chosen option: 1. The default profile (`playwright.config.ts`) runs the suite across four projects: desktop
Chromium, Edge, a Pixel 7 (Chromium) and an iPhone 14 (WebKit). It runs against the API and the Vite
development server, with a fixed 60-second per-test timeout and reporters configured in the file rather than
on the command line. Some files are kept off some projects, none for engine reasons. Two that change shared
data run on `chromium` only (`settings/userSettings.test.ts`, `dashboard/quickActions.test.ts`), and two
desktop-layout files skip the phone projects. The default profile is itself opt-in: the delivery skill asks
before running it, and `run-tests` never runs it as routine verification. The production-like profile
(`playwright.prod.config.ts`) builds the client, serves it from the local preview server and runs the same four
projects against it.

Neither profile is run on every change. The production-like full matrix (`npm run e2e:all:prodlike`) is the
completion gate for a PRD that changed client product code. `npm run e2e:prodlike` is also run on demand
whenever the client production build changes: the Vite configuration, the manifest or PWA set-up,
`.env.production`, or the Docker build arguments. A gate at each such PRD's completion is preferred to
on-demand runs alone, because otherwise a build-only regression is found only when someone happens to
look, and there is no release to hang a pre-release run on.

### Consequences

- Good, because an engine-specific failure is found by the default run rather than by a user.
- Bad, because the two configurations exclude different files. The production-like one drops
  `feedback/errorBoundary.test.ts` on every project, but it does not repeat the default's exclusions of
  `dashboard/quickActions.test.ts` (Edge and both phones) or `rendering/shortLandscapeScroll.test.ts` (both
  phones). The two runs are therefore different slices of the suite, and nothing checks that the lists agree.
- Good, because timeouts and reporters live in the project, so two people running the suite get the same
  behaviour.
- Bad, because nothing enforces the production-like run; it is a completion gate by convention, and a
  change can be declared done without it.
- Bad, because the four projects are two engines: Edge is Chromium and the mobile projects are Chromium and
  WebKit, so Gecko is not covered at all.
- Bad, because the default profile never exercises the built client, so a build-only defect is found by the
  profile that is easiest to skip.

### Confirmation

- Instruction: `.github/skills/prd-delivery/SKILL.md` (step 4.6) runs `npm run e2e:all:prodlike` for a PRD that
  changed client product code, and the report must say so when it is skipped. `.github/skills/run-tests/SKILL.md`
  says not to run the prodlike suite per change.
- Instruction: `.github/instructions/e2e.instructions.md` (Fixtures and config; Reporting and process lifecycle).
  It states the completion gate and the on-demand run. Do not raise `timeout` to chase flakes, and never pass
  `--reporter` from an npm script. `e2e/README.md` (Prodlike config) gives the same rule for people.
- Review: a change to a timeout is made in the configuration and justified there, never passed as a flag.

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

- The shared stack and the test classification: [ADR-0037](0037-end-to-end-tests-run-on-one-shared-stack.md) and
  [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md).
- There is no continuous integration, so nothing runs either profile automatically:
  [ADR-0027](0027-builds-and-deployments-are-run-by-hand.md).
- Source: [PRD-0012](../Future/PRD-0012-playwright-e2e-testing-infrastructure.md) R7 and R8. R8 and decision D6
  call the production-like run an on-demand gate without saying when it is due; this record adds the
  completion gate.
