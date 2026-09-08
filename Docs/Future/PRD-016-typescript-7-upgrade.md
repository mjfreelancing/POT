# PRD-016 — TypeScript 7 Upgrade

> **Classification:** Discovery PRD Draft (gated/future track — not yet executable).
> **Last updated:** 2026-09-08.
> **Predecessor/context:** `PRD-015-react-client-stack-modernisation.md` — DQ-1 deferred TS 7; Inc 12 landed TypeScript **6.0.3** (the highest the toolchain supports today).

## 1. Purpose

Take the POT React client (`Source/Client/pot-react`) from TypeScript **6.0.3** to TypeScript **7.x (native)** once the toolchain is ready, keeping every gate green (cold `tsc -b --force`, lint, unit, build, `npm ci`, E2E, Docker/Azure client image).

This PRD records the current state, the **hard prerequisite** that gates the work, the planned approach, risks, and the decisions still open — so the upgrade can be executed cleanly the moment the blocker clears.

## 2. Current State (verified 2026-09-08 against the live npm registry)

| Item | Value |
| --- | --- |
| Installed TypeScript | `~6.0.3` (`typescript@6.0.3` — resolved from the `~6.0.3` pin in Inc 12) |
| TypeScript `latest` dist-tag | **`7.0.2`** |
| TS 6 stable line | `6.0.2` / `6.0.3` (`6.0.0` was never published; `latest` jumped 5.7 → 7.0.2) |
| typescript-eslint (installed) | `8.70.0` |
| typescript-eslint TS peer range | `>=4.8.4 <6.1.0` — **does not support TS 7** |
| typescript-eslint TS-7 support | **None stable as of 2026-09-08** (canary/alpha only, e.g. `8.70.1-alpha.0`) |
| Runtime peers of TS 7 | Node ≥22.22 (✅ engines + Node 24.7.0); react-router 8 already on ES2022 lib (Inc 11) |
| Repo client gates | cold `tsc -b --force` → lint → unit (`vitest run`) → build → `npm ci` → full E2E matrix → docker/Azure client build |

## 3. The Blocker (why this is a future/gated track)

typescript-eslint — the package that parses/lints TS in this repo (`typescript-eslint@8.70.0`, flat config) — **caps its TypeScript peer at `<6.1.0`**. There is **no stable typescript-eslint release that supports the TypeScript 7 API** yet (only canary/alphas). Forcing TS 7 now (e.g. `--legacy-peer-deps`) would leave linting broken or bypassed, which fails the repo's green-lint gate.

**Trigger to begin implementation:** a **stable** typescript-eslint release (next major, e.g. 9.x) that declares support for TS 7 (peer `typescript` range includes 7.x). Re-check via `npm view typescript-eslint peerDependencies` and `npm view typescript-eslint dist-tags`.

## 4. Goals

- Upgrade `pot-react` to the latest stable TypeScript 7.x with **typescript-eslint upgraded in lockstep** to a TS-7-capable major.
- Align `tsconfig*.json` to TypeScript 7 defaults (the "tsconfig-default alignment" anticipated by PRD-015 Inc 12 / DQ-1).
- Preserve every existing gate as green with **zero behavioural change** (regression-only), matching the PRD-015 discipline.
- Keep the editor/LSP experience intact (typescript-vs-code integration with TS 7).

## 5. Non-Goals

- No React Compiler adoption (PRD-015 DQ-7) — unchanged.
- No re-introduction of the removed `@vite-pwa/assets-generator` devDep (see PRD-015 audit follow-up) unless icons are being regenerated.
- No unrelated dependency bumps beyond the TS 7 + typescript-eslint lockstep (micro-batch installs only; **no broad `npm update`** — repo rule).
- No server/API/DB or Docker-image changes beyond the client toolchain.

## 6. Planned Approach (when the trigger fires)

1. **Verify support** (`npm view`): latest stable TS 7.x, matching typescript-eslint major + its TS peer, plus `ts-api-utils` and any compiler-API peers.
2. **Lockstep install** (micro-batch): `typescript@^7.x` + `typescript-eslint@^<new-major>`; expect possible peer churn (e.g. new `ts-api-utils` line) — resolve cleanly, prefer no overrides unless unavoidable (repo dislikes overrides churn).
3. **tsconfig alignment**: adopt TS 7 defaults where they differ from the current `tsconfig.json`/`tsconfig.app.json`/`tsconfig.node.json`/`tsconfig.e2e.json` (target/lib, module resolution, `erasableSyntaxOnly`-style flags if introduced).
4. **Fix fallout**: run **cold `tsc -b --force`** (incremental masks errors — Inc 0/Inc 4 lesson), fix type errors; run lint and triage any new typescript-eslint findings deliberately.
5. **Gates**: full unit suite → `format:fix` → lint (0 problems expected) → `npm run build` → `npm ci` → **full E2E matrix** (`docker stop pot-react` first; restart after) → Docker/Azure client image build (cold build validates the no-cache path).
6. **Docs/memory**: update PRD-015 tracker row/decisions, PRD-016 → Delivery/Complete, repo + session memory, suggest commit message.

**Watch items:** TS 7 is a native/Go-based toolchain in this timeline — expect: build/lint integration changes, possible `tsc -b` behaviour differences, editor/tsserver changes, and typescript-eslint type-aware rules needing the new API. PRD-015 risk R3 is the template for this.

## 7. Risks

| # | Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- | --- |
| R1 | typescript-eslint can't parse TS 7 if shipped too early (lint breaks the gate) | Certain (if rushed) | High | **Hard trigger:** only start on a stable TS-7-capable typescript-eslint major |
| R2 | TS 7 native compiler changes `tsc -b`/build behaviour or tsconfig defaults | Med | Med | Cold `tsc -b --force` gate; incremental never trusted; dedicated alignment pass |
| R3 | Editor/LSP (typescript-vs-code / TS server) lag behind TS 7 | Med | Low–Med | Keep repo on stable TS; confirm editor extension supports the version before merging |
| R4 | typescript-eslint type-aware rules surface new violations after the major | Med | Low–Med | Deliberate triage in the same increment; lint-only findings fixed or recorded |
| R5 | Unrelated peer churn (ts-api-utils, @typescript-eslint internals) | Med | Low | Lockstep micro-batch; verify `npm ls` clean |

## 8. Open Questions / Decisions

- **DQ-1.1 (package):** Track TS 7 via the `latest` dist-tag (7.0.2+) or pin a specific minor until the ecosystem settles? (Recommended: `~7.0.x` pin first, relax after one patch cycle.)
- **DQ-2.1 (sequencing):** Land TS 7 + typescript-eslint in **one increment** (recommended, lockstep) vs TS 7 first with lint temporarily on the previous parser?
- **DQ-3.1 (scope):** Include the PRD-015 §3 "within-major current" patch refreshes (e.g. `@types/react-dom`, `tailwind-merge`, `lucide`) in the same batch or leave them to routine maintenance? (Recommended: leave them out — keep this increment focused.)
- **DQ-4.1 (baseline):** Any new Rules-of-React / typescript-eslint findings introduced by TS 7 are treated as fix-immediately (matching DQ-4's now-`error` react-hooks rules) — confirm.
- **Audience/ownership:** Single-developer repo; the user is the sole owner/decision-maker — no team assignments needed. (TBD if this ever moves to a team.)

## 9. Assumptions & Constraints Register

- typescript-eslint will ship a stable TS-7-capable major "soon"; until then this PRD is parked.
- The existing `overrides` (react-is, react-day-picker peer pin, serialize-javascript) remain and are unrelated.
- No broad `npm update`; micro-batch installs only.
- Never run git operations (user stages/commits).

## 10. Discovery Exit Criteria (what closes this draft)

1. A **stable** typescript-eslint release exists whose TS peer range includes 7.x (verified via `npm view`).
2. TS 7 + typescript-eslint lockstep install resolves with **no ERESOLVE** and no new override.
3. Cold `tsc -b --force`, lint (0 problems), full unit suite, build, `npm ci`, full E2E matrix, and the Docker/Azure client image build are all green.
4. PRD-015 tracker + this PRD updated; commit message drafted.

## 11. References

- `Docs/Future/PRD-015-react-client-stack-modernisation.md` — Inc 12 (TS → 6.0.3), DQ-1 (hold at 6.0.x until typescript-eslint supports TS 7), risk R3.
- Repo memory `/memories/repo/client-upgrade-audit.md` (Inc-by-Inc audit + version facts).
- Client gates + conventions: `Source/Client/pot-react/DEVELOPER.md`, `.github/copilot-instructions.md`.
