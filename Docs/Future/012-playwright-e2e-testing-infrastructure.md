# Playwright E2E Testing Infrastructure PRD

**Status:** Planning (Active Discovery)
**Priority:** High
**Last Updated:** 2026-05-11
**Feature ID:** 012

## Purpose

Define the minimum, approved infrastructure required to create reliable local Playwright E2E tests for POT before implementing scenario tests.

This remains a planning document. It captures discovery-informed inputs so planning decisions are evidence-based.

## Scope For This Phase

In scope:

- Local E2E execution only.
- Playwright infrastructure for deterministic local runs.
- Login/auth support for all tests.
- Database state strategy (blank vs seeded) per test.
- Support for long cross-feature flows.
- Support for multi-user/concurrency test flows.
- Documentation and instruction guidance for future agents.
- Recording discovery-informed outcomes as planning inputs.

Out of scope:

- CI pipeline integration.
- Cloud-hosted execution.
- Cost-bearing infrastructure changes not required for local execution.

## Discovery-Informed Design Inputs

These items come from discovery spikes and are treated as design inputs unless superseded by later decisions.

- Use a string path for Playwright global setup (`globalSetup: './playwright.globalSetup.ts'`) for ESM-safe resolution.
- Use Testcontainers-managed Postgres in global setup for disposable local database orchestration.
- Parse container URI and map connection values into `DATABASE__*` environment variables expected by backend configuration.
- Execute `Pot.Data.Migrations` from global setup before tests.
- Run migrations with `dotnet run --no-launch-profile --project ...` to avoid launch profile side effects.
- Force both `DOTNET_ENVIRONMENT=Production` and `ASPNETCORE_ENVIRONMENT=Production` for consistent E2E host behavior.
- Standardize on npm-first execution modes (`e2e`, `e2e:smoke`, `e2e:edge`, `e2e:mobile`, `e2e:all`, etc.).
- Define browser/device matrix coverage for Chromium, Edge, mobile Chrome, and mobile Safari.
- Prefer line reporter for non-interactive runs to keep terminal execution non-blocking.
- Include Playwright browser install flow as an explicit prerequisite for required runtime binaries.

## Requirements

### R1. Local-Only Execution

- Tests must run locally on developer machines.
- Supported local modes:
  - App via local dev server.
  - App via local Docker stack.
- No CI requirements in this phase.

### R2. Authentication Baseline

- Every E2E test must run as an authenticated user unless a test explicitly validates unauthenticated behavior.
- Login setup must be repeatable and low friction for writing new tests.
- Infrastructure must support both:
  - single-user login,
  - multiple concurrent logged-in users in one test.

### R3. Explicit Data State Per Test

Each test must declare one of:

- Blank database start state.
- Seeded database start state.

Additional rules:

- State setup must be deterministic.
- Tests must not depend on leftover data from previous tests.
- Cleanup behavior must be defined for each state mode.
- Seeded scenarios use local machine date as the baseline for time-based calculations.

Approved seeded-data approach:

- Maintain a pre-prepared sample database export/import artifact anchored to a reference date.
- Seed tests by importing that artifact through existing server APIs (no new server endpoints).
- After import, run server-side renewal/accrual operations to advance data from the reference date to the current local machine date.
- Tests query the API after renewal/accrual to retrieve expected values.
- UI assertions compare displayed values against API-returned values (no hardcoded expectations).

Rationale for local-date approach:

- Server performs all calculations, ensuring expected values are correct regardless of test run date.
- Tests derive expectations from actual API responses, eliminating risk of stale assertions.
- Reproducibility comes from deterministic import + renewal logic, not clock manipulation.

### R4. Long-Flow Capability

- Infrastructure must support long, cross-feature journeys in one test run.
- Long-flow tests must remain debuggable and recoverable (clear failure points and traceability).

### R5. Concurrency and Multi-Login Capability

- Infrastructure must support tests with two or more simultaneous authenticated sessions.
- Tests must be able to verify auth/session behavior across users and contexts.

### R6. Maintainability for Future Agents

- The intended testing design must be documented clearly enough that a new agent can implement tests without re-deciding architecture each time.
- Instruction guidance for E2E test authoring is required in this phase.

Approved documentation locations:

- Planning and rationale: `Docs/Future/012-playwright-e2e-testing-infrastructure.md`.
- Day-to-day E2E usage guide: `Source/Client/pot-react/e2e/README.md`.
- Agent authoring rules (POT-specific): `.github/instructions/playwright-e2e.instructions.md` with applyTo `Source/Client/pot-react/e2e/**`.

Guidance on reusable vs project-specific instructions:

- For this phase, keep instructions POT-specific in-repo.
- If patterns prove stable across projects, promote later to a reusable baseline outside this repo.

### R7. UI Expectation Source Strategy

- UI E2E tests may use API responses as the source of truth for expected values shown in the UI.
- This is explicitly allowed because server/domain calculations are validated by server tests, while UI E2E validates rendering and user flow correctness.

Required assertion pattern:

- Contract assertion: verify required API fields and shape exist.
- Rendering assertion: verify the UI displays values derived from that API payload.

Guardrails:

- Do not import UI formatting helpers into tests for expected values when that would mirror the exact rendering implementation.
- Prefer expectations derived from raw API payload fields and stable test-side mapping rules.
- Keep at least one reconciliation-style scenario per critical flow (for example import -> renewal/accrual -> UI display).

Accrual testing guidance:

- After seed/import and renewal/accrual steps, query the relevant API response used by the page.
- Build expected values from that payload.
- Assert that table/card/form values in the UI match those expectations.

## Decisions

### D1. Test Visibility Mode (Headless vs Headed)

Decision:

- Approved: Option 1.
- Headless is default; headed is available for debugging and demos.

Operational requirement:

- Scripts must support both modes so a developer can switch without changing test code.

### D2. Database Orchestration Method

Decision:

- Approved with constraint: no server changes allowed.

Selected approach:

- Use Testcontainers-managed disposable Postgres for isolation.
- Do not mutate existing manual-test databases.
- Seed using import artifact through existing server APIs.
- Use defined start date + renewal/accrual operations for deterministic time-based datasets.

Open implementation detail:

- Exact command sequence for import + renewal/accrual needs to be documented during infrastructure build.

### D3. Should We Use Testcontainers?

Decision:

- Approved: adopt Testcontainers now for local E2E database orchestration.

Rationale:

- Clean disposable database lifecycle per run.
- Avoid mutating existing manual test databases.
- Better reproducibility for local E2E.

### D4. Login Strategy

Decision:

- Approved: login once per parallel worker (not per test, not per suite).

Selected approach:

- Use Playwright **worker-scoped** fixtures to authenticate once per worker.
- Each parallel worker gets its own authenticated storage state, saved to `.auth/{workerId}.json`.
- Within a worker, all tests reuse that storage state (test-scoped fixture usage per test's needs).
- Prefer API-based authentication over UI login for speed (use existing server `/api/auth/login` endpoint).
- Keep explicit auth-flow tests that perform full UI login when validating login behavior.

Constraint:

- Worker-scoped auth must not hide tests that require unique user/session setup (multi-role tests use separate fixtures).
- Each worker is isolated; parallel workers can have different authenticated users without conflict.

## Proposed Architecture (Draft, Aligned to Approved Decisions)

Design direction informed by discovery:

- Keep E2E tests under top-level `e2e` directory.
- Add shared helpers for:
  - auth state bootstrap (worker-scoped fixtures),
  - data state setup (test-scoped fixtures for blank/seeded),
  - multi-session/multi-role management (POM fixtures),
  - teardown/cleanup (automatic via Testcontainers + fixture cleanup).
- Add local execution categories:
  - smoke,
  - core,
  - long-flow,
  - concurrency,
  - auth.

### Directory Structure

Target structure:

```
e2e/
  .auth/                    # gitignored, auto-generated per worker
    1.json                  # storage state for worker 1
    2.json                  # storage state for worker 2
    # etc.
  fixtures/
    auth.ts                 # worker-scoped auth fixture (API-based login)
    db.ts                   # test-scoped database fixture (import/seeding/renewal)
    pom/
      AdminPage.ts          # Page Object Model for admin user
      UserPage.ts           # Page Object Model for regular user
  tests/
    smoke/
      appSmoke.test.ts
    core/
      accountFilter.test.ts
    long-flow/
      crossFeatureJourney.test.ts
    concurrency/
      multiSessionScenarios.test.ts
    auth/
      loginFlow.test.ts     # explicit UI login tests
```

### File Descriptions

- **`playwright.config.ts`**: Planned main config to define global setup, projects, and base run configuration.
- **`playwright.globalSetup.ts`**: Planned global orchestration to start Testcontainers Postgres, map `DATABASE__*`, and run migrations.
- **`fixtures/auth.ts`**: Planned worker-scoped fixture for API-based auth and per-worker storage state.
- **`fixtures/db.ts`**: Planned test-scoped fixture for import/renewal/accrual setup.
- **`fixtures/pom/`**: Planned Page Object Model fixtures for multi-role and concurrency scenarios.
- **`.auth/`**: Planned generated storage states, to be gitignored once auth fixture lands.

## Implementation Patterns: Global Setup + Worker Fixtures + Storage State

This section documents the approved architectural patterns based on best practices from Playwright and Testcontainers documentation.

### Pattern 1: Container Lifecycle via Global Setup

**When**: `playwright.globalSetup.ts` runs once per test run, before any workers start.

**How**:

- Testcontainers-managed Postgres starts in global setup.
- Connection URI is stored in environment variable or provided to workers via config.
- Container remains alive for all tests in that run.

**Cleanup**: Playwright automatically stops containers and cleans up after all workers complete.

**Example**:

```typescript
// playwright.globalSetup.ts
import { PostgreSqlContainer } from "@testcontainers/postgresql";

export default async (config: FullConfig) => {
  const postgres = await new PostgreSqlContainer("postgres:16")
    .withDatabase("pot_e2e")
    .withUsername("test_user")
    .withPassword("test_pass")
    .start();

  process.env.DATABASE_URL = postgres.getConnectionUri();
  // Store reference for cleanup in globalTeardown
};
```

### Pattern 2: Worker-Scoped Auth Fixture

**When**: Once per parallel worker, during worker startup (before any test in that worker).

**How**:

- Each worker authenticates via API request context (not UI).
- Uses a unique test account per worker (via `test.info().parallelIndex`).
- Saves authenticated storage state to `.auth/{workerId}.json`.
- All tests in that worker reuse the same storage state.

**Why API over UI**: Faster setup, no UI flakiness, aligns with R7 (API-driven expectations).

**Example**:

```typescript
// fixtures/auth.ts
import { test as base } from "@playwright/test";
import path from "path";

type AuthFixtures = {
  authenticatedStoragePath: string;
};

export const test = base.extend<{}, AuthFixtures>({
  authenticatedStoragePath: [
    async ({ request }, use) => {
      const workerId = test.info().parallelIndex;
      const authDir = path.join(__dirname, "../.auth");
      const authFile = path.join(authDir, `${workerId}.json`);

      // Authenticate via API
      const response = await request.post("/api/auth/login", {
        data: {
          email: `test_user_${workerId}@example.com`,
          password: "test_password",
        },
      });

      // Save storage state
      const context = await request.context();
      await context.storageState({ path: authFile });

      await use(authFile);
    },
    { scope: "worker" },
  ],
});
```

### Pattern 3: Test-Scoped Database Fixture

**When**: Per test (or per describe block if configured for fixture reuse within a group).

**How**:

- Each test declares its data state: blank or seeded.
- Fixture imports artifact via existing server APIs if seeded.
- Runs renewal/accrual to advance data to local machine date.
- Test runs with fresh, isolated data.
- Cleanup happens automatically (container is disposable).

**Example**:

```typescript
// fixtures/db.ts
export const test = base.extend<DbFixtures>({
  dbBlank: async ({ request }, use) => {
    // Test starts with blank database (no import needed)
    await use();
  },
  dbSeeded: async ({ request }, use) => {
    // Import seeded artifact
    await request.post("/api/import/artifact", {
      data: { artifactName: "sample-export-2026-05-11.export" },
    });

    // Advance data from reference date to local date
    await request.post("/api/renewal-and-accrual/process");

    await use();
  },
});
```

### Pattern 4: Multi-Role Testing with POM Fixtures

**When**: Test requires multiple authenticated users interacting simultaneously.

**How**:

- Define Page Object Model fixtures for each role (AdminPage, UserPage, etc.).
- Each POM fixture creates a separate browser context with its own storage state.
- Fixtures can depend on worker auth state or create role-specific contexts.
- Satisfies R5 (concurrency and multi-login).

**Example**:

```typescript
// fixtures/pom/pages.ts
type POMs = {
  adminPage: AdminPage;
  userPage: UserPage;
};

export const test = base.extend<POMs>({
  adminPage: async ({ browser }, use) => {
    const context = await browser.newContext({
      storageState: "e2e/.auth/admin.json",
    });
    const page = await context.newPage();
    const pom = new AdminPage(page);
    await use(pom);
    await context.close();
  },
  userPage: async ({ browser }, use) => {
    const context = await browser.newContext({
      storageState: "e2e/.auth/user.json",
    });
    const page = await context.newPage();
    const pom = new UserPage(page);
    await use(pom);
    await context.close();
  },
});
```

### Pattern 5: Storage State Location and Gitignore

**Convention**:

- Primary auth storage: `Source/Client/pot-react/e2e/.auth/`
- Per-worker states: `.auth/1.json`, `.auth/2.json`, etc. (auto-generated)
- Per-role states: `.auth/admin.json`, `.auth/user.json` (can be pre-generated for explicit multi-role tests)

**Gitignore entry**:

```
e2e/.auth
```

**Rationale**: Auth state contains sensitive cookies/tokens; should never be committed.

### Pattern 6: Data Import Artifact Management

**Artifact Location**: `Data/Exports/pot-{ISO8601-timestamp}.export`

**Approach**:

- Maintain a single "golden" export at a known reference date (e.g., 2026-01-01).
- During fixture setup, import the artifact via API.
- Run server renewal/accrual to advance all time-based calculations to local machine date.
- Result: Deterministic seeded state that works across leap years, future dates, etc.

**Why this works**:

- Server does all calculations (R7: API-driven expectations).
- No clock manipulation needed.
- Reproducible from reference date to any local date.

## PRD 011 Scenario Coverage Intent

PRD 011 scenarios remain the target E2E scenario set.

Execution intent:

- Implement in batches after infrastructure baseline is complete.
- Start with high-value filter/navigation scenarios.
- Add long-flow, concurrency, and auth-specific scenarios after baseline stability.

## Implementation Phases (Approval-Gated)

### Phase A: Requirements Finalization

- [x] Approve D1 (headless default, headed optional).
- [x] Approve D2 with no-server-changes constraint.
- [x] Approve D3 (adopt Testcontainers now).
- [x] Approve D4 (login once per worker where suitable).
- [x] Approve start-date policy: use local machine date for seeded dataset baseline.

### Phase B: Infrastructure Baseline

- [ ] Implement `playwright.globalSetup.ts` to start Testcontainers Postgres.
- [ ] Implement migration orchestration from global setup.
- [ ] Implement production environment forcing for migration + server startup.
- [ ] Configure `playwright.config.ts` global setup wiring.
- [ ] Define local npm run modes, including browser/device matrix.
- [ ] Add and maintain E2E README (`Source/Client/pot-react/e2e/README.md`) as a living operations guide.
- [ ] Implement `fixtures/auth.ts` with worker-scoped API-based authentication.
- [ ] Implement `fixtures/db.ts` with test-scoped import/renewal workflow.
- [ ] Implement `fixtures/pom/` directory with Page Object Models for each role.
- [ ] Implement storage state management (`.auth/` directory, gitignore).
- [ ] Add POT-specific E2E instruction file (`.github/instructions/playwright-e2e.instructions.md`).

### Phase C: Scenario Test Delivery

- Implement PRD 011 scenarios in agreed order.
- Validate each scenario with explicit pass criteria.

## Acceptance Criteria For This Planning PRD

- Local-only scope is explicit and enforced.
- CI is explicitly excluded from this phase.
- Headless/headed mode decision is approved and documented.
- Testcontainers adoption decision is approved and documented.
- No-server-changes constraint for orchestration is documented.
- Seeded-data approach (import artifact + start-date renewal/accrual) is documented.
- Auth, data-state, and concurrency requirements are approved before scenario implementation.
- Documentation locations for humans and agents are explicit.
- **Implementation patterns section documents:** container lifecycle, worker-scoped auth, test-scoped data, multi-role POM fixtures, storage state conventions, and data import workflow.
- **D4 updated to clarify:** auth happens at worker scope with API-based login preference.
- **Directory structure documented** so future agents understand file organization.
- **All decisions grounded in Playwright and Testcontainers best practices** (documented via library consultation).

Planning quality checkpoint for this PRD:

- Discovery-informed inputs are explicitly recorded as design drivers.
- The plan clearly separates:
  - approved design direction,
  - planned infrastructure work,
  - scenario delivery work that follows baseline completion.

## Discussion Notes

- This PRD is planning and governance for implementation.
- Infrastructure implementation can proceed only within the approved constraints above.
