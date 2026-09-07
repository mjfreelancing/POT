# React Client Stack Modernisation — Feasibility Audit & PRD

**Status:** In Progress — Inc 0–4 committed; Inc 5 (lucide) & Inc 6 (zod 4) validated; date-fns 4 split → Inc 8
**Priority:** Medium
**Last Updated:** 2026-09-07
**Feature ID:** 015
**Scope:** Client only — `Source/Client/pot-react` (React 19 + TypeScript SPA). No server, database, or Docker-infra changes beyond what a client toolchain bump requires.
**Audience:** Solo developer / reviewer (lean, decision-oriented).

## Execution Status & Progress Log

_Living tracker — update this section after every increment/commit._

### Decisions taken (2026-09-03)

- Scope: full modernisation to live `latest`, **incl. Vite 8 + React Router 8**.
- Node baseline: **24.7.0** (local) + Docker client image **`node:24-alpine`**.
- **DQ-4:** `react-hooks/set-state-in-effect` **deferred → `warn`** (23 sites) — must be revisited before completion (see Open Follow-ups).
- Removed dead devDeps `eslint-plugin-react` + `eslint-plugin-jsx-a11y` (unused; their peer ranges capped at ESLint `^9` and blocked the ESLint 10 install).
- Each committed batch validated locally with `npm run e2e:all:dev`.

### Decisions taken (2026-09-07)

- **Inc 5 split (date-fns 4 deferred).** Discovered `react-day-picker@8.10.2` (still v8 until Inc 8) declares a hard peer `date-fns ^2.28.0 || ^3.0.0`. Installing date-fns 4 makes `npm ci` fail with ERESOLVE — and the Docker client build runs `RUN npm ci` — so date-fns 4 is **not** durably shippable while react-day-picker is on v8 (the app itself was functionally green: cold tsc, unit 690, build all passed). → **Inc 5 = lucide-react 1.41 only** (date-fns pinned back to `^3.6.0`, `npm ci` green). **date-fns 4 moves to ride with the react-day-picker major (Inc 8)**, which removes the date-fns dependency.
- lucide-react target revised **1.40.0 → 1.41.0** (current `latest`, same major). Deprecated `-Icon` aliases renamed to canonical names across 12 files (`ui/*`: accordion, checkbox, dialog, dropdown-menu, input-otp, select, sheet, sidebar; plus `EnrichedDatePicker`, `ExpenseDetails`, `IncomeDetails`, `POTSettingsSheet`). Aliases are deprecated-but-present in 1.41, so this is forward-cleanup rather than a compile-block.
- Inc 5 (lucide part) validated: cold `npx tsc -b --force` exit 0 · unit suite 690/690 · `format:fix` · lint (24 warnings / 0 errors) · `npm run build` exit 0 · `npm ci` exit 0.
- **Inc 6 (zod 4.5.4) validated.** Added the §13 pre-work form-schema parse guard tests first (5 files / 31 tests + shared `tests/shared/schemaAssertions.ts` `flattenIssues` helper) for the uncovered form schemas (userFormSchemas 0%, budgetReminders 25%, changePassword 33%, expense 56%, income 70%) — green on zod 3, then migrated. Canonical migration across the 18 zod files: the ONLY hard break was `required_error`/`invalid_type_error` → `{ error }` (zod 4.5.4 keeps a classic v3-compat layer for the rest); also `z.nativeEnum(X)` → `z.enum(X)` (merged in v4), `z.string().email('…')` → `z.email('…')`, `.nonempty('…')` → `.min(1, '…')`. refine/superRefine `{ message }` + `z.ZodIssueCode` remain accepted (classic layer) → left as-is, messages locked by guard tests. `@hookform/resolvers` 5.9.1 already zod-4-capable (no bump). Gates: cold tsc exit 0 · unit **721/721** (142 files) · format:fix · lint (24w/0e) · build exit 0 · `npm ci` exit 0.

### Increment status

| Inc | Scope                                                                                                            | Status         | Validated gates                                                        |
| --- | ---------------------------------------------------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------- |
| 0   | Node 24 baseline — Docker image `node:24-alpine`, `engines >=22.22`, `@types/node` 24                            | ✅ Committed   | type:check, lint, unit 679, docker image build (Azure args)            |
| 1   | Foundation refresh — React 19.2.8 + all in-range deps                                                            | ✅ Committed   | type:check, lint, `tsc -b && vite build`, unit 679, docker image build |
| 2   | Lint & format refresh — eslint 9.39, typescript-eslint 8.69, prettier 3.9.6, import-sort 14, react-refresh 0.5.6 | ✅ Committed   | lint, type:check, unit 679, E2E                                        |
| 3   | Test stack — vitest 4.1.11, jsdom 30, jest-dom 7, faker 10, testcontainers 12 + config rework                    | ✅ Committed   | unit 679, coverage (45.13% / 34.7% branch — no shift vs baseline), E2E |
| 4   | Lint correctness — ESLint 10.9.1, @eslint/js 10, react-hooks 7.1.1                                               | ✅ Committed   | format:fix, lint (24 warnings / 0 errors), type:check, unit 679        |
| 5   | Utilities — lucide-react 1.41 (date-fns 4 split out → pairs with Inc 8)                                        | ✅ Validated   | cold tsc, unit 690, format:fix, lint (24w), build, npm ci |
| 6   | zod 4 — 4.5.4, canonical idioms + §13 form-schema guard tests                                                      | ✅ Validated   | cold tsc, unit 721, format:fix, lint (24w), build, npm ci |
| 7   | recharts 3                                                                                                       | ⛔ Not started | —                                                                      |
| 8   | react-day-picker 10 (+ date-fns 4, absorbed from Inc 5)                                                          | ⛔ Not started | —                                                                      |
| 9   | @tanstack/react-table 9                                                                                          | ⛔ Not started | —                                                                      |
| 10  | Vite 6→8                                                                                                         | ⛔ Not started | —                                                                      |
| 11  | react-router 7→8                                                                                                 | ⛔ Not started | —                                                                      |
| 12  | TypeScript 6→7                                                                                                   | ⛔ Not started | —                                                                      |

### Open follow-ups (must resolve before modernisation is declared done)

1. **`react-hooks/set-state-in-effect`: 23 warnings across 19 files** (DataTable, ThemeProvider, AccountsPage, useAccountEditor, LoginForm, PasswordResetDialog, OtpVerificationForm ×4, SignupDialog, AccrualsContext, ExpensesPage, ExpenseForm ×2, IncomesPage, IncomeForm, ProjectionsPage ×2, ChartControls, POTSettingsSheet, use-mobile, use-short-viewport, useDelayedValue). Fix deliberately (matchMedia hooks → `useSyncExternalStore`; forms/pages/contexts → render-time state adjustment), gated by their tests. [DQ-4]
2. `DataTable.tsx` React-compiler warning: "Compilation Skipped: Use of incompatible library".
3. Open decisions: DQ-1 (TS 6 first, then 7), DQ-2 (`@daypicker/react` vs compat name), DQ-3 (coverage — measured: no re-baseline needed), DQ-5 (run all 13 increments vs stop after safe set), DQ-7 (no React Compiler opt-in).
4. Coverage-audit pre-work for the risky increments (6–9): zod form-schema guard tests **done** (Inc 6 — 5 files/31 tests + shared `flattenIssues` helper); remaining pre-work — ProjectionChart render smoke (Inc 7), real `ui/calendar` render (Inc 8), real `DataTable` render (Inc 9).

### Per-increment gate (from §6.1)

type:check → **cold type-check (`npx tsc -b --force` — incremental `tsc` masks errors in unchanged files, but the no-cache Docker/Azure build always cold-builds)** → lint → unit suite (`npx vitest run`) → build → docker image build; E2E (`npm run e2e:all:dev`) per committed batch.

### Manual visual checkpoints (E2E-blind)

The automated gates (679 unit tests + Playwright E2E) assert **behaviour**, not **appearance/feel**. Run the relevant manual smoke before declaring an increment merged. Boot via `docker-start-client-server` (client 5175, API 5241) or the prodlike flow, and check both a normal-user and an admin session plus mobile-width resize where noted.

| Checkpoint                          | Why manual is needed                                                        | What to eyeball                                                                                                                                                                                                                                              |
| ----------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Any committed batch (baseline)**  | Nothing visual should change on pure dependency/tooling bumps               | Quick click-through: sidebar nav, Accounts/Expenses/Incomes lists, a create/edit sheet, date picker, dropdowns, toasts, theme toggle, mobile width. Establishes the baseline so later visual regressions are attributable to the increment that caused them. |
| **Inc 5** (date-fns 4, lucide 1.40) | Icon renames can swap in a canonical icon with subtly different art         | Scan screens for icons: buttons, nav, empty states, toasts, table headers, dialogs.                                                                                                                                                                          |
| **Inc 7** (recharts 3)              | Chart rendering / tooltip / a11y-layer changes are visual                   | Projections page + dashboard: chart draws correctly, hover tooltips, legend, series show/hide, responsiveness at narrow widths.                                                                                                                              |
| **Inc 8** (react-day-picker 10)     | Heavily customised calendar (classNames, ARIA, Chevron slot)                | Every date surface: create/edit expense & income, date filters, Today/Clear actions, range selection, keyboard navigation, mobile.                                                                                                                           |
| **Inc 9** (@tanstack/react-table 9) | DataTable API rewrite feeds all 5 tables                                    | Sorting, row selection + bulk actions, filters, mobile card-vs-table toggle, column headers, empty/loading states, link navigation from rows.                                                                                                                |
| **Inc 11** (react-router 8)         | Core routing rewrite                                                        | Navigate every route incl. guards (auth/roles), deep-link + refresh on each URL, back/forward, the account-filter URL flows (PRD-011).                                                                                                                       |
| **Inc 10** (Vite 8)                 | Build/dev toolchain                                                         | One prodlike run: app loads, no console errors, PWA registers, sensible chunks load, dev HMR sanity.                                                                                                                                                         |
| **Full regression — before "done"** | Final state after open follow-ups (esp. the 23 `set-state-in-effect` fixes) | All screens, both roles, desktop + mobile, PWA install/update, plus prodlike & Azure deploy smoke.                                                                                                                                                           |

> **Revision 2026-09-03 — version data verified against the live npm registry (`npm view`).** The original Section 3 table below was built from `npm outdated`, which in this environment returns **stale cached packuments** and under-quotes `latest`. Verified `latest` (npm `latest` dist-tag) and scope changes:
>
> - Confirmed within-major/current: react **19.2.8**, @types/react 19.2.18, @types/react-dom 19.2.6, tailwindcss/@tailwindcss/vite **4.3.3**, axios 1.20.0, @tanstack/react-query 5.102.8, react-hook-form 7.87.0, @hookform/resolvers 5.9.1, zustand 5.0.15, tailwind-merge 3.6.0, sonner 2.0.8, input-otp 1.5.0, react-error-boundary 6.1.4, @playwright/test 1.62.1, vite-plugin-pwa 1.3.0, @radix-ui (in-range), prettier 3.9.6, typescript-eslint 8.69.0.
> - **Newly-identified majors now in scope (absent from the original table):** Vite **6→8** (8.2.2 — pairs with @vitejs/plugin-react 6.1.1; @tailwindcss/vite 4.3.3, vitest 4.1.11, and vite-plugin-pwa 1.3.0 all peer-support Vite 8), react-router **7→8** (8.3.1; peer react ≥19.2.7; engine node ≥22.22), ESLint **9→10** (10.9.1; flat-config only), @testing-library/jest-dom **6→7** (7.0.1), jsdom **26→30** (30.0.1), @faker-js/faker **9→10** (10.6.0). lucide-react latest is **1.40.0**.
> - **Node prerequisite (NEW):** the active local Node is v20.18.1, below the engine floor for react-router 8 (≥22.22.0), Vite 8 / ESLint 10 (≥20.19 / ≥22.13), and jest-dom 7 (≥22). The machine already has Node 22.14.0 and **24.7.0** installed (nvm4w). → **Inserted Increment 0 = align the client toolchain to Node ≥22.22 (recommend 24.7.0)** before installing true `latest`; confirm the Docker client image and any CI Node version match.
> - Increment changes: `react-router` 7→8 and `vite` 6→8 each become their own gated increment (Vite 8 groups with build/test-tooling; React Router 8 is a core-routing increment). Effort in §11 should be read with ~+2–6 days for these additions.

## 1. Purpose

Audit whether the POT React client can be brought up to the latest published versions of its runtime and toolchain dependencies, quantify the work and risk, and define a phased, gated path to get there without a big-bang cutover. Goal: eliminate version drift now so future upgrades stay cheap and safe. **Full modernisation is the end state; the path is incremental, with each step independently deployable.**

## 2. Executive Summary

### Problem statement

`pot-react` has drifted. It pins to ranges from ~Feb–Jun 2025 (React 19.1, Vite 6.2, TS 5.7, zod 3, Vitest 3, Tailwind 4.1, react-table 8, react-day-picker 8, recharts 2, date-fns 3, lucide 0.x). As of 2026-09-02 the published "latest" for twelve of those are **new major versions**, including two genuine API rewrites and a native-compiler TypeScript release. Every month of drift compounds the eventual migration cost and widens the delta against upstream breaking changes.

### Verdict

**Feasible — no show-stoppers.** React itself is _not_ the blocker (React is on 19.2.x within the same major; there is no React 20 on the registry yet). The blockers are the major-version jumps across the rest of the stack — including, after live re-verification on 2026-09-03, Vite 6→8 and React Router 7→8 (see revision note at top):

- **Highest risk / largest effort:** `@tanstack/react-table` 8 → 9 (headless API rewrite, `TFeatures` generics through the shared `DataTable` layer) and `react-day-picker` 8 → 10 (CSS/classNames/ARIA/prop rewrite behind custom calendar wrappers).
- **Medium:** zod 3 → 4 (schema + RHF error-message rework), TypeScript 5.7 → 7 (config defaults + native toolchain coordination), react-router 7 → 8 (core routing), Vite 6 → 8 (build/dev toolchain), ESLint 9 → 10 + eslint-plugin-react-hooks 5 → 7 (new Rules-of-React violations to triage).
- **Low:** Vitest 4 + jsdom 30 + jest-dom 7 + faker 10 + coverage config, recharts 3, date-fns 4, lucide 1.40, misc ESLint tooling majors.

Both high-risk items sit behind **shared component seams** (`components/table/*`, `components/ui/calendar.tsx` + `components/picker/*`), which concentrates the blast radius — but neither is a pure "bump" and both need their own dedicated increments with full test + E2E gates.

### Proposed approach

Execute **thirteen gated increments — Inc 0 to Inc 12** (Section 6), lowest-risk first, each merged and validated independently before the next starts. No increment requires another to merge, so each can ship to Docker/prodlike on its own.

### Success criteria

- All dependencies at latest published versions (or explicitly deferred with a reason recorded here).
- `npm run type:check`, `npm run lint`, `npm test`, and `npm run build` green after every increment.
- Client unit suite (136 files / 679 tests at the 2026-09-03 baseline) + Playwright E2E (41 files) pass after every increment.
- Zero behaviour change in shipped UI after each increment (regression-only) — a visual/behaviour diff is a failed increment.

## 3. Current State vs Target

Snapshot verified against the **live npm registry** on 2026-09-03 (`npm view <pkg> dist-tags`, `latest` tag). "Jump" = major-version crossings. (The earlier `npm outdated` snapshot of 2026-09-02 was based on stale cached packuments and under-quoted several `latest` values — it is superseded here.)

| Package                                         | Installed             | Latest                | Jump                  | Effort | Risk                  |
| ----------------------------------------------- | --------------------- | --------------------- | --------------------- | ------ | --------------------- |
| react / react-dom                               | 19.1.0                | 19.2.8                | minor                 | S      | Low                   |
| @types/react / @types/react-dom                 | 19.1.8 / 19.1.6       | 19.2.18 / 19.2.6      | minor                 | S      | Low                   |
| vite                                            | 6.4.1                 | 8.2.2                 | **major ×2**          | M      | **Med**               |
| @vitejs/plugin-react                            | 4.6.0                 | 6.1.1                 | **major**             | S–M    | Med (pairs w/ Vite 8) |
| tailwindcss / @tailwindcss/vite                 | 4.1.11                | 4.3.3                 | minor                 | S      | Low                   |
| @tanstack/react-table                           | 8.21.3                | 9.2.4                 | **major**             | L      | **High**              |
| react-day-picker                                | 8.10.1                | 10.0.1                | **major ×2**          | M      | **Med–High**          |
| react-router                                    | 7.13.2                | 8.3.1                 | **major**             | M      | **Med–High**          |
| zod                                             | 3.25.76               | 4.5.4                 | **major**             | M      | Medium                |
| typescript                                      | 5.7.3                 | 7.0.2                 | **major ×2 (native)** | M–L    | **Med–High**          |
| typescript-eslint                               | 8.36.0                | 8.69.0                | minor                 | S      | Low                   |
| vitest / @vitest/ui / @vitest/coverage-istanbul | 3.2.4                 | 4.1.11                | **major**             | S–M    | Medium                |
| jsdom                                           | 26.1.0                | 30.0.1                | **major**             | S–M    | Low–Med               |
| @testing-library/jest-dom                       | 6.6.3                 | 7.0.1                 | **major**             | S      | Low–Med               |
| @faker-js/faker                                 | 9.8.0                 | 10.6.0                | **major**             | S      | Low (test factories)  |
| recharts                                        | 2.15.4                | 3.10.1                | **major**             | S–M    | Low–Med               |
| date-fns                                        | 3.6.0                 | 4.4.0                 | **major**             | S      | Low                   |
| lucide-react                                    | 0.525.0               | 1.40.0                | **major**             | S–M    | Low–Med               |
| eslint / @eslint/js                             | 9.31.0                | 10.9.1 / 10.0.1       | **major**             | S–M    | Medium                |
| eslint-plugin-react-hooks                       | 5.2.0                 | 7.1.1                 | **major ×2**          | S–M    | Medium                |
| eslint-plugin-react-refresh                     | 0.4.20                | 0.5.6                 | major                 | S      | Low                   |
| eslint-plugin-simple-import-sort                | 12.1.1                | 14.0.0                | major                 | S      | Low (autofix)         |
| globals                                         | 15.15.0               | 17.12.0               | major                 | S      | Low                   |
| prettier                                        | 3.5.3                 | 3.9.6                 | minor                 | S      | Low                   |
| @testcontainers/postgresql                      | 11.14.0               | 12.1.0                | **major**             | S      | Low (E2E infra)       |
| @playwright/test                                | 1.59.1                | 1.62.1                | minor                 | S      | Low                   |
| axios                                           | 1.14.0                | 1.20.0                | minor                 | S      | Low                   |
| @tanstack/react-query                           | 5.83.0                | 5.102.8               | minor                 | S      | Low                   |
| react-hook-form                                 | 7.60.0                | 7.87.0                | minor                 | S      | Low                   |
| @hookform/resolvers                             | 5.1.1                 | 5.9.1                 | minor                 | S      | Low (supports zod 4)  |
| zustand                                         | 5.0.6                 | 5.0.15                | minor                 | S      | Low                   |
| tailwind-merge                                  | 3.3.1                 | 3.6.0                 | minor                 | S      | Low                   |
| react-error-boundary                            | 6.0.0                 | 6.1.4                 | minor                 | S      | Low                   |
| sonner / input-otp / vite-plugin-pwa            | 2.0.6 / 1.4.2 / 1.2.0 | 2.0.8 / 1.5.0 / 1.3.0 | minor                 | S      | Low                   |
| @types/node                                     | 22.19.19              | 24.x line             | — (align to runtime)  | S      | Low                   |

> Notes: React stays on 19 (no React 20 published). Radix, @testing-library/react/user-event, and @radix-ui packages have only in-range updates — batch into Inc 1. @testing-library/jest-dom (7) and @faker-js/faker (10) ride the test-stack increment (Inc 3). The client toolchain runs Node 24.7.0 after Inc 0 (react-router 8 requires Node ≥22.22; Vite 8 / ESLint 10 require ≥20.19 / ≥22.13; jest-dom 7 requires ≥22). @types/node is aligned to the Node 24 major rather than the registry `latest` (26.x), which matches no installed runtime.

## 4. Feasibility Assessment by Area

### 4.1 Where the risk actually lives

**`@tanstack/react-table` 8 → 9 — HIGH (architectural rewrite).**
v9 is a new headless API: `useReactTable` → `useTable`, row models move into a required `features` registry, `getCoreRowModel()` is gone, `ColumnDef`/`Row`/`Table` gain a leading `TFeatures` generic, rendering prefers the `FlexRender` component, and the `declare module` augmentation pattern in `DataTableHeader.tsx` is replaced by a meta/feature-registry model. A temporary `useLegacyTable` bridge exists under `@tanstack/react-table/legacy` for staged cutover.
Blast radius is concentrated but wide: `components/table/*` (DataTable, DataTableHeader, DataTableContent, DataTableColumnHeader, dataTableColumnFactories, dataTableUtils), all 5 feature tables (Accounts, Approvals, Expenses, Incomes, Users), `lib/expenseTableRowUtils.tsx` / `incomeTableRowUtils.tsx`, plus **test files that import the `Row`/`Column` types** (`genericRowBuilder.ts`, `DataTableContent.test.tsx`, `DataTableColumnHeader.test.tsx`, `expenseTableRowUtils.test.tsx`, `incomeTableRowUtils.test.tsx`). Type-generic churn propagates everywhere; behaviour/DOM churn is limited because table tests mock the react-table instance.

**`react-day-picker` 8 → 10 — MED–HIGH.**
v10 (package `@daypicker/react`) is v9's cleanup: `classNames` keys changed (`day` → `day_button`, `cell` → `day`, `day_selected` → `selected`, …), old nav props replaced (`fromMonth` → `startMonth`, `initialFocus` → `autoFocus`, `fromDate/toDate` → `hidden`), `IconLeft`/`IconRight` → single `Chevron` slot, formatters must return strings, ARIA labels changed. Direct hit on `components/ui/calendar.tsx` (heavy v8-style `classNames` map + `components={{ IconLeft, IconRight }}`), `components/picker/EnrichedCalendar.tsx`, `EnrichedDatePicker.tsx`. Test churn is limited because `EnrichedCalendar.test.tsx` mocks the Calendar component, but `EnrichedCalendar.VisualBehavior.test.tsx` queries real DOM (`button[aria-selected="true"]`) and any ARIA-label assertions must be re-verified. **Go straight to v10 semantics; do not pause on v9** (v10 removes all v9-compat APIs).

**TypeScript 5.7 → 7 — MED–HIGH (toolchain, not runtime).**
6.0 (last JS-based release) is the bridge to 7.0 (native Go/tsgo, 8–12× faster). New default config (`strict`, explicit `types: []`, `rootDir: ./`, `stableTypeOrdering`) plus deprecations that become hard errors in 7 (`baseUrl` → rewrite into `paths`, `moduleResolution: node`, etc.). **Tooling constraint:** `typescript-eslint` cannot `import "typescript"` against the native 7.0 yet (no programmatic API until 7.1) — it must run against TS 6 via `@typescript/typescript6`. This repo's `eslint.config.mjs`/`eslint.config.js` depend on `typescript-eslint`, so TS7 and typescript-eslint must move in lockstep. Recommendation: land TS 6.0.x first (safe, tooling-compatible), then schedule TS 7 as its own decision after typescript-eslint confirms 7.x support.

**zod 3 → 4 — MEDIUM (mechanical but broad).**
Used in 18 source files: API entity parsing (`data/*.ts`, ~11 files) and RHF form schemas (`features/*/schemas`, plus userSettings sections) with `zodResolver`. Breaking changes that bite this shape of code: error-message customisation rework (`message` param → `{ error }`; `invalid_type_error`/`required_error` dropped), `z.enum(SomeTsEnum)` replacing `z.nativeEnum()`, `.coerce` missing-key semantics (add `.default()` where relied on), `superRefine` `ctx.path` removal, top-level `z.email()`/`z.uuid()` replacing string methods. `@hookform/resolvers` 5.x supports zod 4. No official codemod (community `zod-v3-to-v4` exists). Data-schema tests (`tests/data/*.test.ts`) exercise parsing and will catch regressions.

**Vitest 3 → 4 + jsdom 27 — LOW–MEDIUM.**
Component tests (jsdom + testing-library) are largely stable, but: `coverage.all`/`coverage.extensions` removed (the `test:ui` script uses `--coverage.all`; must switch to explicit `coverage.include`), default excludes simplified, pool options renamed, `vi.fn()` default name change affects snapshots, jsdom 27 swaps its CSS selector engine and `element.click()` fires `PointerEvent`. Bump `vitest`, `@vitest/ui`, `@vitest/coverage-istanbul`, and `jsdom` **in lockstep** in one increment.

### 4.2 Where the risk is LOW (batch freely)

- **recharts 3:** `components/ui/chart.tsx` (shadcn wrapper) + `ProjectionChart.tsx` custom Tooltip. Main fixes: a11y layer now on by default, `TooltipProps` → `TooltipContentProps` typing. v2 code ports with minor edits.
- **date-fns 4:** type-level only for the functions used (`format`, `parseISO`, `addDays`, `addMonths`, `subMonths`, `addYears`, `subYears`). ESM-first — invisible under Vite.
- **lucide 1.x:** brand icons removed (Facebook/GitHub/LinkedIn/…) — **verified none are imported**. Main churn is deprecated `-Icon`-suffixed aliases in shadcn `ui/*` files (`XIcon`, `CheckIcon`, `ChevronDownIcon`, `CircleIcon`, `MinusIcon`, `PanelLeftIcon`, …) → canonical names; community `lucide-react-codemod` available.
- **ESLint majors:** `simple-import-sort` 12→14 and `globals` 15→17 are autofix/data-only; `react-refresh` 0.4→0.5 is ESM-only + new `configs.vite` preset (config edit).
- **eslint-plugin-react-hooks 5→7:** preset-shape churn + **new Rules-of-React violations** (compiler-aware: `set-state-in-effect`, ref validation, `use`/`useEffectEvent` rules) surface as lint findings to triage across the component tree. Lint-only; no runtime risk.

## 5. Upgrade Inventory — Code-Specific Blast Radius

| Area               | Files at risk                                                                                                     | Driver                                          |
| ------------------ | ----------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| Shared table layer | `src/components/table/*` (7 files)                                                                                | react-table 9                                   |
| Feature tables     | AccountsTable, ExpensesTable, IncomesTable, UsersTable, PendingApprovalsTable                                     | react-table 9 (types)                           |
| Row util libs      | `src/lib/expenseTableRowUtils.tsx`, `incomeTableRowUtils.tsx`, `tableRowUtils.ts`                                 | react-table 9                                   |
| Table tests        | `tests/components/table/*`, `tests/lib/*TableRowUtils.test.tsx`, `tests/shared/rows/genericRowBuilder.ts`         | react-table 9 (types/mocks)                     |
| Calendar shell     | `src/components/ui/calendar.tsx`                                                                                  | day-picker 10 (classNames/components)           |
| Calendar wrappers  | `src/components/picker/EnrichedCalendar.tsx`, `EnrichedDatePicker.tsx`                                            | day-picker 10 (props/ARIA/Chevron)              |
| Calendar tests     | `tests/components/picker/EnrichedCalendar*.test.tsx`                                                              | day-picker 10 (ARIA/DOM)                        |
| API/entity schemas | `src/data/*.ts` (~11 files) + `tests/data/*.test.ts`                                                              | zod 4                                           |
| Form schemas       | `src/features/{accounts,expenses,incomes,users}/schemas/*`, `src/features/userSettings/sections/*/*Schema.ts`     | zod 4                                           |
| Charts             | `src/components/ui/chart.tsx`, `src/features/projections/components/ProjectionChart.tsx`, `src/lib/chartUtils.ts` | recharts 3                                      |
| Icons              | shadcn `src/components/ui/*.tsx` + feature icons                                                                  | lucide 1.x (alias renames)                      |
| Lint config        | `eslint.config.js`, `eslint.config.mjs`                                                                           | react-hooks 7 / react-refresh 0.5 / TS6+        |
| Test config        | `vitest.config.ts`, `package.json` (`test:ui` uses `--coverage.all`)                                              | Vitest 4 / jsdom 27                             |
| TS config          | `tsconfig.app.json`, `tsconfig.node.json`, `tsconfig.json`                                                        | TS 6/7 defaults (`types`, `rootDir`, `baseUrl`) |

## 6. Proposed Phasing (Gated Increments)

Each increment = one working-tree change set, run through the full gate (6.1) and shipped independently. Ordering is risk-ascending and dependency-aware. Inc 0–3 are safe; the deliberate majors follow (Inc 4–12).

- **Inc 0 — Toolchain baseline (env).** Align the client toolchain to Node ≥22.22 → **Node 24.7.0** (switched 2026-09-03 via nvm4w; verify with `node --version`). Align `@types/node` to the Node 24 major. Confirm npm 11 installs the lockfile cleanly and the Docker client image / any CI Node version match.
- **Inc 1 — Foundation refresh (low).** React 19.1 → 19.2.8, `@types/react(-dom)`, and every in-range minor/patch **not reserved for a later major**: axios, react-query, react-hook-form, @hookform/resolvers, zustand, tailwindcss 4.3.3 + @tailwindcss/vite, tailwind-merge, Radix, sonner, input-otp, react-error-boundary, @testing-library/react, user-event, @playwright/test, vite-plugin-pwa 1.3.0. Held back for their own majors: react-router (stays 7.13.2 → Inc 11), vite/plugin-react (→ Inc 10), eslint/@eslint/js 9.39 (→ Inc 4), jest-dom (→ Inc 3), date-fns/lucide (→ Inc 5), zod/recharts/day-picker/react-table (→ Inc 6–9). Proves the pipeline end-to-end before any major.
- **Inc 2 — Lint & format refresh (low).** typescript-eslint 8.69.0, prettier 3.9.6, globals 17.12.0, simple-import-sort 14.0.0 (autofix-only), react-refresh 0.5.6 (flat `configs.vite`), eslint-config-prettier/plugin-prettier, eslint/@eslint/js 9.39.5 (in-range; ESLint 10 deferred to Inc 4). Run `eslint --fix` + prettier.
- **Inc 3 — Test stack (low–med).** vitest 4.1.11 + `@vitest/ui` + `@vitest/coverage-istanbul` + jsdom 30.0.1 + `@testing-library/jest-dom` 7.0.1 + `@faker-js/faker` 10.6.0 + `@testcontainers/postgresql` 12.1.0 in one lockstep change. Rework `vitest.config.ts` coverage (`include` not `all`), add `test.exclude` for `e2e/**` (fixes the bare-`vitest run` E2E sweep), update the `test:ui` script, fix snapshot/selector/click-event fallout. Capture the coverage baseline before this increment.
- **Inc 4 — Lint correctness major (med).** ESLint 10.9.1 + @eslint/js 10.0.1 + eslint-plugin-react-hooks 7.1.1 (pairs with ESLint 10); triage the new Rules-of-React findings deliberately (set-state-in-effect, ref validation, etc.). Lint-only; no runtime risk.
- **Inc 5 — Utilities: lucide 1.41 (low–med).** lucide-react 1.40.0 → **1.41.0** (current latest); rename deprecated `-Icon` aliases to canonical names in shadcn `ui/*` + feature icons; verify the ARIA default on icons doesn't alter labels we set. **date-fns 4 deferred** — react-day-picker v8 pins `date-fns ^2||^3` (breaks `npm ci`/Docker), so date-fns 4 lands with the react-day-picker major in Inc 8.
- **Inc 6 — zod 4.5.4 (med).** Data schemas + form schemas + error-utility issue-code reads; update `zodResolver` usage if needed; run `tests/data/*` first as the safety net.
- **Inc 7 — recharts 3.10.1 (low–med).** `chart.tsx` + `ProjectionChart` custom tooltip; adjust `accessibilityLayer` default if behaviour changes; verify `ChartConfig` typing.
- **Inc 8 — react-day-picker 10.0.1 (med–high) + date-fns 4.** Update `calendar.tsx` classNames/components to v10 keys + package decision (DQ-2: `@daypicker/react` vs compat alias); port `EnrichedCalendar`/`EnrichedDatePicker`; update VisualBehavior + ARIA assertions. **Also bump date-fns 3.6 → 4.4.0 here** (rdp ≥9 drops the date-fns peer, unblocking the date-fns 4 absorbed from the original Inc 5 scope).
- **Inc 9 — @tanstack/react-table 9.2.4 (high).** Migrate shared `components/table/*` to `useTable` + `features` + `FlexRender`, thread `TFeatures` types, replace module augmentation with the v9 meta/feature model; update row util libs, the 5 feature tables, and type-importing tests. Optional staged path: `useLegacyTable` bridge first, then port off it.
- **Inc 10 — Vite 6 → 8 (med).** vite 8.2.2 + @vitejs/plugin-react 6.1.1; confirm @tailwindcss/vite 4.3.3, vite-plugin-pwa 1.3.0, vitest 4.1.11 peers; review dev/build config (proxy, HMR, manualChunks) and build output.
- **Inc 11 — react-router 7 → 8 (8.3.1) (med–high, core routing).** Apply route/API breaking changes; exercise all routes via E2E; requires Node ≥22.22 + react ≥19.2.7 (satisfied after Inc 0/1).
- **Inc 12 — TypeScript 6 → 7 (med–high, schedule decision).** Land TS 6.0.x + tsconfig-default alignment + typescript-eslint lockstep first; schedule TS 7.0 native after typescript-eslint supports the 7.x API (DQ-1).

### 6.1 Per-increment gate (exit criteria)

1. `npm run type:check` clean.
2. `npm run lint` clean (plus prettier applied; repo runs `npm run format:fix`).
3. Full unit suite green — `npx vitest run --dir tests` (136 files / 679 tests at the 2026-09-03 baseline) — target increment's tests run first.
4. Client build green (`npm run build`).
5. E2E green against a freshly started stack (`docker-start-client-server`, then `e2e:all:dev` chromium/edge/mobile) — run at least the features touched by the increment.
6. Manual smoke on affected surfaces (e.g. date pickers/charts/tables for Inc 8/9; routing for Inc 11).
7. No behavioural/visual diff beyond the upgrade (screenshot compare or targeted manual review).

> If any gate fails, the increment reverts or is fixed forward **within that increment** — never carry a red increment into the next.

## 7. Deployment Strategy (Staged)

- Each increment merges separately and ships via the existing flows (client Docker build/tag → prodlike → Azure). The client is a PWA with `registerType: 'prompt'`, so updates are already user-controlled/versioned — no silent-force complication.
- Ordering means the first staged deployments (Inc 0–3) are near-zero-risk and build confidence; the deliberate majors (Inc 4–12) each arrive as one reviewable, revertable change.
- Because increments are independent, work can be paused after any increment with the codebase in a fully green state (e.g. stop after Inc 5 if appetite runs out).

## 8. Risk Register

| #   | Risk                                                                                                                                                | Likelihood       | Impact  | Mitigation                                                                                                                                |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| R1  | react-table 9 type-generic churn ripples through all table code/tests                                                                               | Certain          | High    | Shared `DataTable` seam isolates most changes; dedicated increment; mock-based table tests limit DOM churn; `useLegacyTable` escape hatch |
| R2  | day-picker 10 classNames/ARIA changes alter calendar look/behaviour                                                                                 | High             | Med     | Update wrapper in one place; VisualBehavior + ARIA test pass is the gate; visual smoke on date pickers                                    |
| R3  | TS 7 native toolchain breaks `typescript-eslint`/editor integration                                                                                 | High (if rushed) | Med     | Land TS 6 first; gate TS 7 on typescript-eslint 7.x API support; keep `@typescript/typescript6` alias path                                |
| R4  | Vitest 4 coverage semantics shift coverage numbers                                                                                                  | Certain          | Low     | Explicit `coverage.include`; re-baseline coverage expectation deliberately, not silently                                                  |
| R5  | react-hooks 7 Rules-of-React findings are numerous                                                                                                  | Med              | Low–Med | Dedicated triage in Inc 9; lint-only, no runtime risk                                                                                     |
| R6  | zod 4 error-message rework degrades form UX messages                                                                                                | Med              | Low–Med | Preserve custom messages; `tests/data` + form tests guard parsing; visual smoke on forms                                                  |
| R7  | lucide 1.x icon renames missed in obscure spots                                                                                                     | Low              | Low     | `lucide-react-codemod` + build/type-check catches missing exports; brand icons verified absent                                            |
| R8  | jsdom 27 selector-engine changes flip test queries                                                                                                  | Med              | Low     | Whole-suite run in Inc 3 catches fallout; fix queries in the same increment                                                               |
| R9  | Unrelated drift: overrides in `package.json` (`react-is`, `react-day-picker` peer pin, `serialize-javascript`) stop being needed/behave differently | Med              | Low     | Review overrides each increment; remove stale ones deliberately                                                                           |

## 9. Non-Goals

- No upgrade to React 20 or any framework change (React 20 is not yet published).
- No Vite or React Router major beyond the ones the 2026-09-03 revision adds (Vite 6→8, React Router 7→8), each executed as its own gated increment.
- No server/API/DB changes, and no Docker infrastructure changes beyond the client toolchain.
- No feature work, UI redesign, or refactor beyond what each upgrade requires (avoid scope creep during migrations).
- No adoption of React Compiler itself — only its lint rules (via react-hooks 6/7 presets).
- No silent coverage re-baselining: any coverage threshold change is an explicit, reviewable decision.

## 10. Open Questions / Decisions

- **DQ-1 (schedule):** Hold TypeScript at 6.0.x for now and schedule 7.0 (native) only after `typescript-eslint` confirms 7.x support? (Recommended: yes.)
- **DQ-2 (package):** Adopt `@daypicker/react` as the new package name, or stay on the `react-day-picker` compat name? (Recommended: `@daypicker/react`, it's the current primary package.)
- **DQ-3 (coverage):** Accept re-baselined client coverage numbers under Vitest 4's `coverage.include` semantics, or invest to hold the current threshold?
- **DQ-4 (lint):** Treat new react-hooks 7 Rules-of-React violations as fix-immediately or defer as warnings during Inc 9?
- **DQ-5 (scope ceiling):** Is the full thirteen-increment programme in scope now, or should the plan stop after Inc 5 (safe increments) with majors re-scoped per-quarter? (2026-09-03: full true-latest scope incl. Vite 8 + React Router 8 confirmed; ceiling still open.)
- **DQ-6 (E2E harness):** Include `@testcontainers/postgresql` 11 → 12 (used by the Playwright/E2E harness) in Inc 3, or handle it separately when the E2E infra is next touched?
- **DQ-7 (React Compiler):** Out of scope per Non-Goals — confirm before any increment that no compiler opt-in is expected.

## 11. Estimated Effort (lean, dev-days)

| Increment                                       | Effort     |
| ----------------------------------------------- | ---------- |
| 0 Toolchain baseline (Node 24)                  | 0.25–0.5   |
| 1 Foundation refresh                            | 0.5–1      |
| 2 Lint & format refresh                         | 0.5–1      |
| 3 Test stack (Vitest 4 + jsdom 30 + jest-dom 7) | 1–2        |
| 4 Lint correctness (ESLint 10 + hooks 7)        | 1–2        |
| 5 Utilities (date-fns 4, lucide 1.40)           | 1–2        |
| 6 zod 4                                         | 2–3        |
| 7 recharts 3                                    | 1–2        |
| 8 react-day-picker 10                           | 2–4        |
| 9 @tanstack/react-table 9                       | 3–6        |
| 10 Vite 6→8                                     | 1–2        |
| 11 react-router 7→8                             | 2–4        |
| 12 TypeScript 6→7                               | 2–4        |
| **Total (excluding gates/E2E overhead)**        | **~17–34** |

Gate/E2E runs add ~1 day per major increment. Expect the overall programme to be roughly **4–8 focused working sessions** if done in dedicated slots, spread across separate merges.

## 12. Discovery Exit Criteria

This document converts from a _discovery draft_ into an _implementation plan_ only when:

1. DQ-1…DQ-7 are answered (each answer recorded here).
2. Increment order is confirmed (default: risk-ascending as listed).
3. A decision is recorded on the TS 6 vs TS 7 target and the react-day-picker package name.
4. A baseline snapshot (this document's Section 3) is committed as the "before" record.

Until then, no increment in Section 6 is a committed commitment — they are candidate work items pending this review.

## 13. Regression Test Coverage Audit (2026-09-03)

Run before any upgrade change. Baseline: **136 test files / 679 tests, all green** via `npx vitest run --dir tests`.

> Method notes: a bare `vitest run` (and the `npm run test` script) **sweeps in the Playwright E2E files** (`e2e/**/*.test.ts`) because `vitest.config.ts` has no `include`/`exclude` — there is currently **no clean root-level unit-only command**. This is fixed in Inc 3 (`test.exclude`). The audit used `--dir tests`. One pre-existing defect found and fixed: `UserMenu.test.tsx` mocked a stale path (`UserSettingsSheet`; module renamed to `POTSettingsSheet`) → 4 "Route render failed" failures; mock path corrected.

Overall Istanbul coverage (all `src/**`): **45.1% stmts / 34.7% branch / 47.4% funcs / 45.0% lines**. Low headline is structural, not a defect: vendored shadcn `ui/*`, and leaf/table components that page tests deliberately mock, drag it down. There is **no coverage threshold configured**, so coverage is a guard-signal, not a gate. The meaningful signal for this plan is per-module coverage of what each increment touches:

| Inc                        | Primary guard tests (measured coverage)                                                                                                                                                                                                                                                                                                  | Verdict                                                    | Pre-increment action                                                                                                                                               |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 0 Node/toolchain           | none needed (env)                                                                                                                                                                                                                                                                                                                        | n/a                                                        | verify `node -v` = 24.7.0, npm 11, clean `npm ci`                                                                                                                  |
| 1 Foundation refresh       | whole suite 679; lib 100%, data ~97–100%, api hooks ~90–100%                                                                                                                                                                                                                                                                             | ✅ Adequate                                                | none                                                                                                                                                               |
| 2 Lint & format            | `type:check` + `lint` (autofix-only majors)                                                                                                                                                                                                                                                                                              | ✅ Adequate                                                | none                                                                                                                                                               |
| 3 Test stack               | whole suite 679 + coverage config rewrite                                                                                                                                                                                                                                                                                                | ✅ Adequate                                                | **capture coverage baseline before & after** (Inc 3 changes `coverage.all` → `include`)                                                                            |
| 4 ESLint 10 + hooks 7      | `lint` (new Rules-of-React findings)                                                                                                                                                                                                                                                                                                     | ✅ Adequate (lint-only)                                    | none beyond triage                                                                                                                                                 |
| 5 lucide 1.41 (date-fns 4 → Inc 8) | `lib/dateUtils` 100%, `lib/chartUtils` 100% (stay on date-fns 3 — unchanged), icon-bearing components + `tsc` catches renamed exports                                                                                                                                                                                                      | ✅ Adequate                                                | none                                                                                                                                                               |
| 6 zod 4                    | `src/data/*` ~97–100% (`tests/data/*.test.ts`); form schemas: accounts 100, expense 56, income 70, users `userFormSchemas` 0, userSettings budgetReminders 25 / changePassword 33 (userDetails & siteDetails 100); forms: ExpenseForm 90, IncomeForm 86, AccountForm 91                                                                  | 🟡 Good entity guard; **form-schema gap**                  | add small parse tests for `expenseFormSchema`, `incomeFormSchema`, `userFormSchemas`, budgetReminders/changePassword schemas before Inc 6                          |
| 7 recharts 3               | `ui/chart.tsx` 0%, `ProjectionChart` 0%, `ChartControls`/details 0%; `useProjectionChartData` 100%, ProjectionsPage 83%; E2E projections/dashboard                                                                                                                                                                                       | 🟡 Thin at unit level (only E2E/integration renders chart) | add a render smoke test for `ProjectionChart` (and `ui/chart` ChartContainer) before Inc 7                                                                         |
| 8 react-day-picker 10      | `picker/EnrichedCalendar` 82%, `EnrichedDatePicker` 100%, `EnrichedCalendar.VisualBehavior` (real DOM, 2 tests); **`ui/calendar.tsx` only 33% — the uncovered lines (62–65) are exactly the `IconLeft`/`IconRight`/Chevron slot v10 replaces**                                                                                           | 🟡 Good wrapper guard; **calendar.tsx slot gap**           | add a real `DayPicker` render test through `ui/calendar` (single + range, classNames + nav) before Inc 8                                                           |
| 9 @tanstack/react-table 9  | **`DataTable` 0%, `dataTableColumnFactories` 0%, `DataTableHeader` 0%; feature tables (Accounts/Expenses/Incomes/Users/Approvals) all 0%**; `DataTableColumnHeader` 100, `DataTableContent` 100, `dataTableUtils` 100, `BulkActionsBar` 100, `lib/*TableRowUtils` 100; real-render only via Playwright E2E (crud/table/mobile/rendering) | 🔴 **Thinnest guard on the highest-risk increment**        | **add a real-render DataTable test** (columns via factories, sorting, row selection, custom `rowId`) before Inc 9 — this is the single most valuable pre-work item |
| 10 Vite 6→8                | `npm run build`, full suite 679, E2E; config-only                                                                                                                                                                                                                                                                                        | ✅ Adequate with gates                                     | none                                                                                                                                                               |
| 11 react-router 7→8        | routes `AppRoutes` 65% + navigation/auth E2E (6 files)                                                                                                                                                                                                                                                                                   | 🟡 Medium                                                  | run all-route E2E before & after; add route smoke for protected/role-gated routes if time                                                                          |
| 12 TypeScript 6→7          | `type:check` (`tsc -b`), `lint`, full suite 679                                                                                                                                                                                                                                                                                          | ✅ Adequate (type-level)                                   | none                                                                                                                                                               |

**Conclusion:** logic layers the majors touch are well guarded (data/zod ~97–100%, lib 100%, hooks ~90–100%). The gaps that matter are in the **UI seams the two highest-risk upgrades rewrite**:

1. **Inc 9 (react-table 9):** no unit test renders a real `DataTable` → add one first.
2. **Inc 8 (day-picker 10):** the `ui/calendar.tsx` slot lines are uncovered → add a real render test first.
3. **Inc 6 (zod 4):** add small parse tests for the currently-uncovered form schemas.
4. **Inc 7 (recharts 3):** add a `ProjectionChart` render smoke test.

Recommended policy: **before each risky increment (6–12), land its guard test first (green), then migrate, then re-run.** This converts Section 6's per-increment gate into test-first increments without changing the ordering.

---

### Companion index

Registered in [`Docs/Future/README.md`](README.md) as feature **015**.
