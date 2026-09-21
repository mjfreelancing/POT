# Client Dependency Drift Maintenance (Post-Modernisation) PRD

**Status:** Proposed
**Priority:** Low
**Last Updated:** 2026-09-21
**Feature ID:** 017
**Scope:** Client only — `Source/Client/pot-react`. No server, database, or Docker-infra changes beyond what a client toolchain bump requires.
**Audience:** Solo developer / reviewer (lean, decision-oriented).
**Predecessor:** [PRD 015 — React Client Stack Modernisation](PRD-015-react-client-stack-modernisation.md) (Complete, 2026-09-21). This is the routine-maintenance continuation of that programme, not a reopening of it.

## 1. Purpose

Stop the client re-accumulating the drift PRD-015 removed. PRD-015 verified the stack against the live registry on 2026-09-08; thirteen days later nineteen declared packages had moved again. Record that delta as a checklist so the next maintenance pass is a short, gated batch job rather than another full audit — while keeping the incremental discipline that made PRD-015 safe (no big-bang, no broad `npm update`).

## 2. Current State (verified 2026-09-21 via `npm view <pkg> version`)

### 2.1 In-major updates available (safe batch candidates)

| Package                     | Installed | Latest  | Jump  |
| --------------------------- | --------- | ------- | ----- |
| react / react-dom           | 19.2.8    | 19.3.0  | minor |
| @types/react                | 19.2.18   | 19.3.0  | minor |
| @types/react-dom            | 19.2.7    | 19.3.0  | minor |
| react-router                | 8.3.1     | 8.4.0   | minor |
| zod                         | 4.5.4     | 4.6.5   | minor |
| lucide-react                | 1.41.0    | 1.47.0  | minor |
| vite                        | 8.2.2     | 8.3.0   | minor |
| eslint                      | 10.9.1    | 10.11.0 | minor |
| jsdom                       | 30.0.1    | 30.1.0  | patch |
| @tanstack/react-query       | 5.102.8   | 5.103.2 | patch |
| react-hook-form             | 7.87.0    | 7.88.0  | minor |
| react-error-boundary        | 6.1.4     | 6.1.6   | patch |
| @playwright/test            | 1.62.1    | 1.63.0  | minor |
| prettier                    | 3.9.6     | 3.9.8   | patch |
| @testing-library/dom        | 10.4.1    | 10.4.2  | patch |
| eslint-plugin-react-refresh | 0.5.6     | 0.5.7   | patch |
| oxc-transform-react         | 0.145.0   | 0.150.0 | minor |
| cn                          | 0.2.6     | 0.3.0   | minor |

`cn` backs the shadcn v4 `ui/*` layer, so its bump is a registry-consumer change: confirm the `cn` API before moving, and keep `ui/*` registry-owned (never hand-edited).

### 2.2 New majors (each needs its own decision/increment)

| Package                                         | Installed | Latest | Track                                                                                                                |
| ----------------------------------------------- | --------- | ------ | -------------------------------------------------------------------------------------------------------------------- |
| vitest / @vitest/ui / @vitest/coverage-istanbul | 4.1.11    | 5.0.1  | **New major.** Outside PRD-015's scope (that programme targeted Vitest 4). Needs its own gated increment — see §3.   |
| typescript                                      | 6.0.3     | 7.0.2  | [PRD 016](PRD-016-typescript-7-upgrade.md) — gated on a stable typescript-eslint release that supports the TS 7 API. |

### 2.3 Already current (no action)

`@tanstack/react-table` 9.2.4 · `react-day-picker` 10.0.1 · `date-fns` 4.4.0 · `recharts` 3.10.1 (bumped to match the Inc 7 record at PRD-015 closure) · `@faker-js/faker` 10.6.0 · `@testing-library/jest-dom` 7.0.1 · `@vitejs/plugin-react` 6.1.1 · `tailwindcss` / `@tailwindcss/vite` 4.3.3 · `axios` 1.20.0 · `@hookform/resolvers` 5.9.1 · `zustand` 5.0.15 · `sonner` 2.0.8 · `input-otp` 1.5.0 · `vite-plugin-pwa` 1.3.0 · `typescript-eslint` 8.70.0 · `@eslint/js` 10.0.1 · `@testing-library/react` 16.3.3 · `@testing-library/user-event` 14.6.7 · `@testcontainers/postgresql` 12.1.0 · `globals` 17.12.0 · `eslint-plugin-react-hooks` 7.1.1 · `eslint-plugin-simple-import-sort` 14.0.0 · `radix-ui` 1.6.7 · `class-variance-authority` 0.7.1 · `next-themes` 0.4.6 · `jwt-decode` 4.0.0 · `tailwindcss-animate` 1.0.7 · `eslint-config-prettier` 10.1.8 · `eslint-plugin-prettier` 5.5.6.

### 2.4 Deliberately held, not drift

- **`@types/node` 24.13.3** (registry latest 26.6.2) — aligned to the Node 24 runtime floor per PRD-015 §3; a `@types/node` major that matches no installed runtime is noise. Revisit only if the Node baseline moves.
- **`react-day-picker` package name** — stays `react-day-picker`, not `@daypicker/react` (PRD-015 DQ-2).
- **`overrides`** — `react-is`, the `react-day-picker` peer pin, and `serialize-javascript` remain; no override churn.

## 3. Proposed Approach (micro-batch, PRD-015 discipline)

- One working-tree change set per batch, each independently shippable. **Never a broad `npm update`** — PRD-015's key lesson is that a single one-shot update produced ten unexplained test hangs that micro-batches did not reproduce.
- Suggested batching (risk-ascending):
  1. **Test/tooling patches** — prettier 3.9.8, @testing-library/dom 10.4.2, eslint-plugin-react-refresh 0.5.7, react-error-boundary 6.1.6, @tanstack/react-query 5.103.2, react-hook-form 7.88.0, jsdom 30.1.0, oxc-transform-react 0.150.0 (peer of `@vitejs/plugin-react` 6 — verify before installing).
  2. **Runtime minors** — react/react-dom 19.3.0 + @types/react(-dom) 19.3.0 (lockstep), react-router 8.4.0, zod 4.6.5, lucide-react 1.47.0.
  3. **Build/lint minors** — vite 8.3.0, eslint 10.11.0, @playwright/test 1.63.0 (+ `npm run e2e:install` if browser revisions moved).
  4. **Vitest 5** — its own increment: check `vitest.config.ts` (`test.exclude`, `coverage.include`, provider, pool options, snapshot naming) and re-baseline coverage deliberately, exactly as Inc 3 did for Vitest 4.
- **Per-increment gate (PRD-015 §6.1):** cold `npx tsc -b --force` → `npm run format:fix` → `npm run lint` (must stay 0 problems) → full unit suite → `npm run build` → `npm ci` → E2E for the touched areas, with the full matrix before declaring a batch done.
- **Version source of truth is `npm view <pkg> version`** — `npm outdated` returns stale cached packuments in this environment and under-quotes `latest`.
- **`ui/*` stays registry-owned:** the update path is `shadcn add <name> -y -o`, never a hand-edit.
- Check `npm audit` after each batch; the 2026-09-21 closure state is **0 vulnerabilities** and should stay there.

## 4. Non-Goals

- No redoing of PRD-015's major migrations (table, calendar, chart, router, Vite, ESLint, TS).
- No TypeScript 7 work — PRD-016 owns it and remains gated on typescript-eslint.
- No broad `npm update`, no `npm audit fix --force`, no override churn.
- No feature work, UI redesign, or refactor beyond what a bump requires.
- No server/API/DB or Docker-infra changes.

## 5. Risks

| #   | Risk                                                           | Likelihood | Impact | Mitigation                                                                                                     |
| --- | -------------------------------------------------------------- | ---------- | ------ | -------------------------------------------------------------------------------------------------------------- |
| R1  | A batch re-introduces the PRD-015 "one-shot update" hang class | Med        | Med    | Micro-batches only; run the unit suite after each batch; `npm ci` from the lock to confirm reproducibility     |
| R2  | Vitest 5 breaks component tests or coverage config             | Med        | Med    | Dedicated increment; capture coverage before/after (Inc 3 precedent); verify `test.exclude`/`coverage.include` |
| R3  | ESLint 10.11 surfaces new findings                             | Low        | Low    | Deliberate triage; lint stays 0 problems (the DQ-4 `set-state-in-effect` rule is at `error`)                   |
| R4  | Playwright 1.63 needs new browser builds                       | Low        | Low    | Run `npm run e2e:install` after the bump; keep the matrix as the gate                                          |
| R5  | react 19.3 + `@types/react` 19.3 surface type errors           | Low        | Low    | Cold `tsc -b --force` gate (incremental tsc masks errors in unchanged files)                                   |
| R6  | Drift keeps compounding because the pass is deferred           | Med        | Low    | Treat this PRD as a checklist; run it on a cadence (see DQ-1)                                                  |
| R7  | Registry `latest` for a package is a broken/pulled release     | Low        | Low    | Verify `npm view` + `npm ci` green before committing; revert the batch if a gate fails                         |

## 6. Open Questions / Decisions

- **DQ-1 (cadence):** trigger this pass on a schedule (e.g. quarterly) or on demand after a feature milestone?
- **DQ-2 (Vitest 5):** adopt as its own increment now, or ride it with the next test-facing change?
- **DQ-3 (`@types/node`):** keep following the Node runtime floor (current stance), or follow the registry latest?
- **DQ-4 (`cn` 0.3.0):** bump and re-pull `ui/*` from the shadcn v4 registry, or hold until the registry's next cli release?
- **DQ-5 (scope ceiling):** cap a pass at in-major updates only, leaving each new major to its own document (recommended)?

## 7. Validation / Test Considerations

- Gates: PRD-015 §6.1 (cold type-check, lint, unit, build, E2E, `npm ci`, manual smoke on touched surfaces).
- Baselines to compare against (2026-09-21): unit **155 files / 780 tests green**; full E2E matrix **273 passed / 0 failed / 66 skipped**; lint **0 problems**; `npm audit` **0 vulnerabilities**.
- Known monitored flake: `tests/api/interceptors/authInterceptor.test.ts` can fail 1–2 tests under parallel full-suite load and passes standalone — do not block a batch on it.
- PRD-015's regression policy applies: a batch that changes shipped behaviour or appearance is a failed batch.

## Related Documents

- Future index: [Docs/Future/README.md](README.md)
- Predecessor programme: [PRD-015 — React Client Stack Modernisation](PRD-015-react-client-stack-modernisation.md)
- Deferred major: [PRD-016 — TypeScript 7 Upgrade](PRD-016-typescript-7-upgrade.md)
- Client toolchain facts and lessons: `Source/Client/pot-react/DEVELOPER.md`
