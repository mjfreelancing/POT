---
id: ADR-0041
title: End-to-end tests run on a development profile by default, and a production-build profile gates client-changing PRDs
status: accepted
date: 2026-05-11
area: cross-cutting
related-prds: [PRD-0012]
supersedes: [ADR-0043]
---

# ADR-0041 (backfilled): End-to-end tests run on a development profile by default, and a production-build profile gates client-changing PRDs

**Summary:** In the context of a browser suite that is slow enough to be run deliberately and valuable
enough not to skip, facing the choice between one configuration and two, we decided on a development
profile that runs the suite across several browser projects against the development server, and a
production-build profile that runs the same projects against the built client and gates the completion of
every PRD that changes the client, to achieve confidence without paying the strictest cost every time,
accepting that neither profile runs automatically, so the gate is kept by convention.

## Context and Problem Statement

Two things can go wrong that only a browser shows: the interface behaves differently in one engine, and it
behaves differently when built for production rather than served by the development server. The first is
cheap to cover with the same stack; the second needs a built client and a different base URL.

Running the production-like profile on every change would double the slowest gate. Running only the
development profile risks a build-only failure — a base URL, an asset path, a service worker — reaching a
deployment.

When the production-like profile must run was written down two ways. The delivery skills and
`e2e.instructions.md` made it the completion gate for a PRD that changed client product code. `e2e/README.md`
and ADR-0043 called it on demand, when the production build changes and before releases. There are no
formal releases, so "before releases" named no trigger.

## Decision Drivers

- Engine-specific behaviour should be caught before a release, not after.
- The everyday suite must finish in a time someone will accept.
- The production build differs from the development server in ways that matter.

## Considered Options

1. A development profile by default, plus a production-build profile that gates client-changing PRDs.
2. One configuration only, always.
3. Both profiles on every change.
4. A production-build profile run on demand only, when the build changes and before releases.
5. No fixed policy; run whatever is convenient.

## Decision Outcome

Chosen option: 1. The default profile (`playwright.config.ts`) runs the suite across several browser
projects against the API and the Vite development server. The production-build profile
(`playwright.prod.config.ts`) builds the client, serves it from the local preview server and runs the same
projects against it.

Neither profile is run on every change. The production-build full matrix (`npm run e2e:all:prodlike`) is
the completion gate for a PRD that changed client product code. `npm run e2e:prodlike` is also run on
demand whenever the client production build changes: the Vite configuration, the manifest or PWA set-up,
`.env.production`, or the Docker build arguments. A gate at each such PRD's completion is preferred to
on-demand runs alone, because otherwise a build-only regression is found only when someone happens to
look, and there is no release to hang a pre-release run on.

### Consequences

- Good, because an engine-specific failure is found by the default run rather than by a user.
- Good, because a build-only defect has a fixed point at which it is looked for.
- Bad, because nothing enforces the production-build run; it is a completion gate by convention, and a
  change can be declared done without it.
- Bad, because the projects cover two engines, Chromium and WebKit, so Gecko is not covered at all.
- Bad, because the default profile never exercises the built client, so a build-only defect is found by the
  profile that is easiest to skip.

### Confirmation

- Instruction: `.github/skills/prd-delivery/SKILL.md` (step 4.6) runs `npm run e2e:all:prodlike` for a PRD that
  changed client product code, and the report must say so when it is skipped. `.github/skills/run-tests/SKILL.md`
  says not to run the prodlike suite per change.
- Instruction: `.github/instructions/e2e.instructions.md` (the opening Config paragraph) states the completion
  gate and the on-demand run. `e2e/README.md` (Prodlike config) gives the same rule for people.
- Gap: the two configurations exclude different files, so the two runs are different slices of the suite,
  and nothing checks that the lists agree. The production-build profile drops `feedback/errorBoundary.test.ts`
  on every project, but does not repeat the default's exclusions of `dashboard/quickActions.test.ts` (Edge
  and both phones) or `rendering/shortLandscapeScroll.test.ts` (both phones).

## Pros and Cons of the Options

### Two profiles, production build as a completion gate (chosen)

- Good: everyday cost is bounded; the strict run is available, specified and has a trigger.
- Bad: the strict run depends on a person.

### One configuration

- Good: one thing to run and one to reason about.
- Bad: either the everyday run is slow or the production build is never tested.

### Both on every change

- Good: no discipline required.
- Bad: the slowest gate becomes slow twice over, and it will start being skipped.

### Production build on demand only

- Good: the strict run is paid only when the build itself changes.
- Bad: a build-only regression from an ordinary change is found only when someone happens to look, and
  "before releases" names no trigger when there are no releases.

### No policy

- Good: total freedom.
- Bad: the suite's coverage becomes whatever the last person ran, and no one can say what was tested.

## More Information

- Current projects (`playwright.config.ts`): desktop Chromium, Edge, a Pixel 7 (Chromium) and an iPhone 14
  (WebKit).
- Files kept off projects in the default profile, none for engine reasons: two that change shared data run
  on `chromium` only (`settings/userSettings.test.ts`, `dashboard/quickActions.test.ts`), and two
  desktop-layout files skip the phone projects.
- The default profile is itself opt-in: the delivery skill asks before running it, and `run-tests` never
  runs it as routine verification.
- The fixed 60-second per-test timeout and the reporters configured in the file rather than on the command
  line are conventions of the configuration, not of this decision. They make two people's runs behave the
  same; `e2e.instructions.md` (Fixtures and config; Reporting and process lifecycle) and `e2e/README.md`
  state them and the rules that keep them. A change to a timeout is made in the configuration and
  justified there, never passed as a flag.
- The shared stack and the test classification: [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md) and
  [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md).
- There is no continuous integration, so nothing runs either profile automatically:
  [ADR-0031](ADR-0031-builds-and-deployments-are-run-by-hand.md).
- Source: [PRD-0012](../Future/PRD-0012-playwright-e2e-testing-infrastructure.md) R7 and R8. R8 and decision D6
  call the production-like run an on-demand gate without saying when it is due; this record adds the
  completion gate.
