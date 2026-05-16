# Playwright E2E Testing Infrastructure PRD

**Status:** Planning (Active Discovery)
**Priority:** High
**Last Updated:** 2026-05-16
**Feature ID:** 012

## Purpose

Define the minimum, approved infrastructure required to create reliable local Playwright E2E tests for POT before implementing scenario tests.

## Scope For This Phase

In scope:

- Local E2E execution only.
- Playwright infrastructure for deterministic local runs.
- Login/auth support for all tests.
- Database state strategy (blank, site-and-users-only, or fully seeded) per test.
- Support for long cross-feature flows.
- Support for multi-user/concurrency test flows.
- Documentation and instruction guidance for future agents.

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
- Prefer line reporter for non-interactive agent runs to keep terminal execution non-blocking.
- Include Playwright browser install flow as an explicit prerequisite for required runtime binaries.

## Requirements

### R1. Local-Only Execution

- Tests must run locally on developer machines.
- Supported via local Docker stack.
- No CI requirements in this phase.

### R2. Authentication Baseline

- Every E2E test must run as an authenticated user unless a test explicitly validates unauthenticated behavior.
- Login setup must be repeatable and low friction for writing new tests.
- Infrastructure must support both:
  - single-user login,
  - multiple concurrent logged-in users in one test.

### R3. Explicit Data State Per Test

Each test must declare one of three database start states:

**Mode 1: Blank**

- Migrations applied; no site, no users, no financial data.
- Used exclusively for tests that exercise user registration and OTP-based signup flows.
- Auth fixture is not applicable for tests running in this mode (unauthenticated by design).

**Mode 2: Site-and-users-only**

- Site and canonical E2E users exist; no financial data has been imported.
- Established by loading the committed data-only SQL seed file (`e2e/seed/baseline.sql`) into the Testcontainers database after migrations run.
- Used for tests that create their own income/expense entries to assert specific financial behaviors.
- Auth fixture applies normally (users are already present).

**Mode 3: Fully seeded**

- Site and canonical E2E users exist; financial data imported from an export artifact; renewal/accrual run to advance data to local machine date.
- Established by loading the SQL seed file (same as Mode 2), then importing the financial artifact via the server API, then running renewal/accrual.
- Used for projection, filter, and long-flow scenarios that require realistic financial state.
- Auth fixture applies normally.

Additional rules:

- State setup must be deterministic.
- Tests must not depend on leftover data from previous tests.
- Cleanup behavior must be defined for each state mode.
- Fully seeded scenarios use local machine date as the baseline for time-based calculations.

Import constraint:

- The export/import artifact contains financial data only; it does not encode site configuration or user records.
- The site and users must already exist in the database before the import artifact can be applied.
- Modes 2 and 3 therefore require site and users to be established first (either via the golden state or per-test setup).
- Attempting to import a financial artifact into a blank database (no site present) must be rejected by the server; this rejection behavior is a required test scenario in its own right (Mode 1 / blank database, unauthenticated or admin-authenticated import attempt → expect failure response).

Approved setup sequence for modes 2 and 3:

1. After Testcontainers starts and migrations run, load `e2e/seed/baseline.sql` into the container database via `psql`. This file is a `pg_dump --data-only` snapshot of the site and canonical user rows and is committed to the repository.
2. _(Mode 3 only)_ Import the financial export artifact through existing server APIs (no new server endpoints).
3. _(Mode 3 only)_ Run server-side renewal/accrual operations to advance data from the artifact reference date to the current local machine date.
4. Tests intercept the API responses already requested by the client (via `page.waitForResponse()`) to retrieve expected values; no separate API calls are made.
5. UI assertions compare displayed values against API-returned values (no hardcoded expectations).

SQL seed file rules:

- File path: `e2e/seed/baseline.sql`.
- Generated once from a manually prepared database using `pg_dump --data-only --table=<site-and-user-tables>`.
- Committed to the repository; human-reviewable and incrementally extensible.
- Must be regenerated when the canonical site key, usernames, or role assignments change.
- Schema changes that add non-nullable columns without defaults require regeneration; otherwise the file is resilient to additive schema changes.

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

- After seed/import and renewal/accrual steps, intercept the API response the page already requests using `page.waitForResponse()`; register the listener before the navigation or action that triggers the request.
- Build expected values from that intercepted payload.
- Assert that table/card/form values in the UI match those expectations.

### R8. Test Execution Safety Classification

Every test must be explicitly classified as one of three execution-safety classes before it is written. The infrastructure provides one shared API process and one shared Testcontainers database per test run; classification determines whether a test is safe to run concurrently with others.

**Parallel-safe**

- The test calls only GET endpoints or performs UI-only interactions that do not trigger server-side state changes.
- The test does not assert on data that a concurrently-running sibling test could also create, modify, or delete.
- May run freely against shared infrastructure with any number of parallel workers.

**Fixture-managed**

- The test creates, updates, or deletes its own records.
- Must be placed inside a `test.describe.serial()` block so steps are never interleaved with other workers.
- Must clean up created data in `afterEach` teardown (not `afterAll`) so cleanup is guaranteed even when a step fails.
- Sibling parallel-safe tests are unaffected because they do not query the mutation test's records.

**Isolated-sequence**

- The test triggers operations with global side effects: import, accrual, renewal, or any operation that changes aggregate or calculated financial state visible to all tests.
- Must run in a dedicated Playwright invocation (separate npm script) with no other tests competing for shared infrastructure.
- Must reset the database to a known baseline using `applyDatabaseSeedMode()` in `beforeAll` before the sequence begins.
- Must not be included in the same Playwright invocation as parallel-safe or fixture-managed tests.

Additional rules:

- Classification must hold for local parallel execution. CI `workers: 1` serialises all tests but is not a substitute for correct classification; parallelism bugs suppressed by CI serialisation will surface when worker count is increased.
- There are no per-test isolated containers. All workers in a single run share one API process and one database instance.

## Decisions

### D1. Test Visibility Mode (Headless vs Headed)

Decision:

- Approved: Headless is default; headed is available for debugging and demos.

Operational requirement:

- Scripts must support both modes so a developer can switch without changing test code.

### D2. Database Orchestration Method

Decision:

- Approved with constraint: no server changes allowed.

Selected approach:

- Use Testcontainers-managed disposable Postgres for isolation.
- Do not mutate existing manual-test databases.
- Execute data setup through three explicit modes using one deterministic fixture pipeline:
  - Mode 1 (Blank): migrations only; no site, users, or financial data.
  - Mode 2 (Site-and-users-only): migrations plus site-and-user baseline setup.
  - Mode 3 (Fully seeded): mode 2 plus financial import and renewal/accrual.
- Run migrations before any mode-specific setup.
- Keep orchestration concerns here (container lifecycle, mode sequencing, teardown) and keep artifact/source details in D6, D8, and D9.

Open implementation detail:

- Exact fixture entry points, command wrappers, and teardown boundaries per mode need to be documented during infrastructure build.

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

### D5. OTP Delivery Strategy For Local E2E

Decision:

- Approved: retain real OTP generation/verification logic, but replace SMTP destination with a local email sink for OTP-required scenarios.

Selected approach:

- Use a local sink mailbox service (for example MailPit) in the Docker E2E stack.
- Keep existing server OTP flow unchanged:
  - server generates OTP,
  - server queues email,
  - worker sends email,
  - test reads OTP from sink mailbox.
- For seeded-user login paths, OTP is not required because users are pre-created via the SQL seed baseline (`e2e/seed/baseline.sql`).
- For signup/password-reset coverage, tests explicitly exercise OTP flows by reading the sinked email and submitting the OTP through existing endpoints/UI.

Constraints:

- No new server endpoints for test-only OTP retrieval.
- No bypass of OTP verification logic in production code paths.
- Sink mailbox configuration must be local E2E scoped only and not used in normal runtime environments.

### D6. Seed Data Source Strategy

Decision:

- Approved: use a committed data-only SQL seed file for the site-and-users baseline, combined with a separate financial export artifact for fully seeded runs.

Selected approach:

- The export/import artifact contains financial data only; site configuration and user records are not part of the artifact.
- The site and canonical users are established by loading `e2e/seed/baseline.sql` (a `pg_dump --data-only` snapshot) into the Testcontainers database after migrations.
- The SQL seed file is committed to the repository, is human-reviewable, and can be extended over time as test requirements grow.
- Fully seeded runs load the SQL seed file first, then import the financial artifact via the server API, then run renewal/accrual.
- Data freshness is maintained by renewal/accrual operations and targeted per-test additions in setup.

Constraint:

- Automated user-creation coverage is deferred and tracked separately from baseline seeded execution.

### D7. OTP Strategy For E2E

Decision:

- Approved: use sink-mailbox based OTP capture for OTP-required flows.

Selected approach:

- Seeded-user flows rely on pre-created users from the SQL seed baseline (`e2e/seed/baseline.sql`) and do not require OTP.
- Signup/password-reset coverage reads OTP values from the local sink mailbox and submits through existing application paths.

Constraint:

- OTP behavior remains production-faithful and is not bypassed in application code paths.

### D8. Post-Import Setup Strategy

Decision:

- Approved: artifact import plus renewal/accrual as the baseline seeded setup path.

Selected approach:

- No per-run custom bootstrap app is required for baseline seeded runs.
- Setup relies on loading the SQL seed file (site/users), importing the financial artifact, and running existing server-side refresh operations.

Constraint:

- Any additional setup logic must remain deterministic and reproducible from the SQL seed file and financial artifact.

### D9. Site-and-Users Baseline Mechanism

Decision:

- Approved: use a committed data-only SQL seed file (`e2e/seed/baseline.sql`) loaded via `psql` after migrations.

Rationale:

- Testcontainers runs migrations before test fixtures execute; a full `pg_restore` (including schema) would conflict with already-created tables. A `pg_dump --data-only` avoids this entirely.
- The SQL file is small, version-controlled, human-reviewable, and incrementally extensible without new tooling.
- Resilient to most schema changes (additive columns with defaults do not require regeneration).
- No additional server endpoints or bootstrap applications are required.

Constraint:

- The SQL seed file must be regenerated when canonical identities (site key, usernames, roles) change or when non-nullable columns without defaults are added to covered tables.

### D10. Test Isolation Strategy

Decision:

- Approved: three-class classification model (parallel-safe, fixture-managed, isolated-sequence); per-test isolated containers are explicitly rejected.

Selected approach:

- Infrastructure provides one shared API process and one Testcontainers database per run. Isolation is achieved through classification and execution discipline, not container proliferation.
- Parallel-safe tests run freely against shared infrastructure with full worker parallelism.
- Fixture-managed tests serialise within their suite via `test.describe.serial()` and restore baseline state in `afterEach` teardown.
- Isolated-sequence tests run in a dedicated Playwright invocation (separate npm script). The database is reset to a known mode via `applyDatabaseSeedMode()` (from `e2e/helpers/databaseSeed.ts`) in `beforeAll` before the sequence begins.

Rationale:

- Shared infrastructure with classification discipline is simpler to maintain and faster to execute than per-test containers.
- The three classes map cleanly onto test categories: smoke and read-only navigation tests → parallel-safe; CRUD and feature-level mutation tests → fixture-managed; import, accrual, and projection flow tests → isolated-sequence.
- `applyDatabaseSeedMode()` provides the same state guarantees as a fresh container without the startup overhead.

Constraints:

- Any test that triggers import, accrual, or renewal must be placed in an isolated-sequence run; running such a test alongside parallel-safe or fixture-managed tests in the same invocation is not permitted.
- Per-test isolated containers are not approved for any phase of this infrastructure.

Technical reference:

- Worker configuration, code patterns, and the test-class decision guide are in `Source/Client/pot-react/e2e/README.md` under “Test Isolation and Concurrency Model”.
- Classification rules for agents are in `.github/instructions/playwright-e2e.instructions.md` under “Test Isolation Classification”.

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

- Each test declares its data state: blank, site-and-users-only, or fully seeded.
- Modes 2 and 3 load the SQL seed file into the Testcontainers database; mode 3 then imports the financial artifact and runs renewal/accrual.
- Cleanup happens automatically (container is disposable).

**Three fixture modes**:

- `dbBlank`: No site, no users, no financial data. Used for signup/OTP tests and blank-database import rejection tests.
- `dbSiteAndUsers`: SQL seed file loaded; site and canonical users present; no financial data. Used for tests that supply their own financial entries.
- `dbSeeded`: SQL seed file loaded; financial artifact imported via API; renewal/accrual run to local date. Used for financial scenario and projection tests.

**Example**:

```typescript
// fixtures/db.ts
import { execSync } from "child_process";

export const test = base.extend<DbFixtures>({
  dbBlank: async ({ request }, use) => {
    // Migrations applied; no site or users. For signup/OTP and negative-path import tests.
    await use();
  },
  dbSiteAndUsers: async ({ request }, use) => {
    // Load data-only SQL seed: site and canonical users.
    execSync(`psql ${process.env.DATABASE_URL} -f e2e/seed/baseline.sql`);
    // Tests add their own income/expense entries as needed.
    await use();
  },
  dbSeeded: async ({ request }, use) => {
    // Load data-only SQL seed: site and canonical users.
    execSync(`psql ${process.env.DATABASE_URL} -f e2e/seed/baseline.sql`);

    // Import financial artifact via existing server API.
    await request.post("/api/import/artifact", {
      data: { artifactName: "sample-export-2026-05-11.export" },
    });

    // Advance data from reference date to local date.
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

### Pattern 6: Seed Artifacts — SQL Baseline and Financial Export

Two committed artifacts together establish the fully seeded state.

**SQL seed file**

- Path: `e2e/seed/baseline.sql`
- Content: `pg_dump --data-only` snapshot of site and canonical user rows.
- Loaded into the Testcontainers database via `psql` after migrations, before any API calls.
- Committed to the repository; version-controlled and human-reviewable.
- Regenerate when canonical identities change or a breaking schema change affects covered tables.

**Financial export artifact**

- Path: `Data/Exports/pot-{ISO8601-timestamp}.export`
- Content: Financial data only (no site or user records); anchored to a known reference date.
- Imported via the existing server import API after the SQL seed file has been loaded.
- Run server renewal/accrual after import to advance calculations to local machine date.

**Why this works**:

- Migrations run first; the data-only SQL file never conflicts with the schema.
- SQL file is small, reviewable, and extensible as test requirements grow.
- Server does all financial calculations (R7: API-driven expectations).
- No clock manipulation needed; reproducible from reference date to any local date.

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
