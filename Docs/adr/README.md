# Architecture Decision Records

Every significant technical decision in POT is recorded here, one file each, in one numbered series.
A decision that outlives the feature which prompted it belongs here; a binding convention belongs in
[Docs/standards/](../standards/), a product rule belongs in a PRD, and the *process* of writing either
belongs in the skills.

**Why one series.** The reasoning for the system's shape used to live inline in the developer guides,
where a rule, an example and the argument for the rule shared one section. When a decision changed, the
argument was rewritten in place or duplicated, and a later reader could not tell a current decision from
a superseded one. One numbered file per decision gives each a stable address that a guide, an
instruction file and a review comment can all cite, and a `status` field that answers "is this current?"
without reading the body. See `DOCS-WORK-LISTS.md` W4 for the decisions found worth recording.

## When to write one

Raise an ADR when any of these is true:

- it constrains features beyond the one being built;
- it is costly to reverse;
- it crosses components or defines the contract between the client and the server;
- it was a consequential choice made before or during implementation, and the alternatives, rationale
  or trade-offs would be lost if only the current code remained.

Do not raise one for a coding convention ([Docs/standards/](../standards/) holds those), for a product
rule (a PRD holds those), or for an approach that was tried and dropped without shipping — record that
in the "Considered Options" of the ADR that replaced it. When the decision is already in the code, use
the PRDs, dated notes, commit history, guides and source as evidence, and do not present an inference as
established historical fact.

Review the system as it was being designed, not only as it behaves now. A decision can merit an ADR
even when it is old, stable or already encoded in tests. Current code establishes current behavior; it
does not necessarily explain why the system took that shape.

## How to create one

The template lives with the `adr` skill, which is what writes records:
[`.github/skills/adr/references/adr.template.md`](../../.github/skills/adr/references/adr.template.md).

1. Ask the agent to use the `adr` skill, or follow its procedure yourself.
2. Take the next number from the highest `id` in this folder. Numbers are never reused, and a withdrawn
   draft consumes its number. A new record takes the next number whatever its `date`, so numbers follow
   decision date only up to ADR-0053. On 2026-11-06, before the series reached `main`, it was renumbered
   into date order, PRD numbers were padded to four digits, and ADR file names gained the `ADR-` prefix
   that PRD file names already carry. Later the same day, an audit for records focused on a feature
   rather than a decision removed one proposed record that only restated a PRD's requirements, and split
   the PWA record in two, so ADR-0036 to ADR-0046 moved down by one and the split-off record took
   ADR-0046 in date order. That is the one exception to "never reused"; the series had not reached
   `main`. Commit messages and branches from before that use the earlier numbers and file names.
3. Copy the template to `ADR-NNNN-<kebab-case-title>.md` in this folder, keeping the four-digit number.
4. Fill in the front matter: `id`, `title`, `status`, `date` (when the decision was made) and `area`.
   `related-prds`, `supersedes` and `superseded-by` are optional.
5. Write the body in the order the template gives. "Confirmation" must name something mechanical — a
   test, an analyzer, an instruction file or a review rule — because a record nobody applies is prose.
6. Run `docs-check` (`-Update` regenerates the index below) before offering the change for review.

Related ADRs can be grouped as a decision family in an overview or index (for example, API architecture
may include separate records for its contract, concurrency, error handling and security boundaries).
The family is for navigation, not a record that combines independent decisions; keep one decision per
ADR.

## The format

The format is MADR 4, customised. Required sections: Summary, Context and Problem Statement, Considered
Options (at least two, including the status quo when the decision is already in the code), Decision
Outcome, Consequences and Confirmation. Optional: Decision Drivers, Pros and Cons of the Options, More
Information.

Front matter is eight lines: five required fields and three optional links. It is limited to flat
`key: value` pairs and simple lists, because `docs-check` parses it without a YAML library and rejects
anything else.

In ADR-0043 and ADR-0044 the heading's number carries `(legacy)` (`# ADR-0043 (legacy): …`). In every
other record up to ADR-0052, except ADR-0045, it carries `(backfilled)`: each was written after its
decision was in the code. The `id` and `title` fields carry no suffix. The suffix is the only marker, and
the body does not repeat it. Their `date` is when the decision was made: the date of the PRD the record
came from, otherwise the oldest commit in the `v0.1` history that introduced the decision. ADR-0045,
ADR-0053 and every later record carry no suffix.

Target 40–120 lines, with a ceiling of about 150. A longer draft is either several decisions (split it)
or a design document (move the detail to reference or explanation and link it).

## Statuses and lifecycle

| Status | Meaning |
| --- | --- |
| `proposed` | Written down; not yet binding. An ADR raised by a PRD stays `proposed` while that PRD is Planning or In Progress. |
| `accepted` | Binding. A standalone ADR is `accepted` when committed; a PRD's ADR becomes `accepted` when the PRD is Complete. |
| `rejected` | Considered and not adopted. Kept so it is not proposed again without new evidence. |
| `deprecated` | Still recorded, no longer recommended. |
| `superseded` | Replaced by another ADR, named in `superseded-by`. |

Status never tracks implementation progress; the PRD's status does that.

Once accepted, an ADR may only change for a typo, a link, the `status` and `superseded-by` fields, or a
dated clarification that does not change the decision.

## Amending and superseding

- **A narrower change that leaves the decision standing** is a dated clarification line in the record,
  or a new ADR that supersedes it if the decision statement itself changes. There is one mechanism, not
  two: a record that needs more than a clarification is superseded.
- **Supersede** when the decision statement is no longer true: set the old record to `superseded`,
  point `superseded-by` at the new one, and give the new one `supersedes`.
- Never rewrite an accepted decision in place. The record of what was decided is the point.

## Index

<!-- BEGIN GENERATED INDEX -->
_Generated from the ADR front matter by `.github/scripts/docs-check/docs-check.ps1`. Do not edit by hand._

## By area

### Process

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0031](ADR-0031-builds-and-deployments-are-run-by-hand.md) | Builds, tests and deployments are run by hand; there is no continuous integration | accepted |

### Cross-cutting

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md) | End-to-end tests run on one shared stack against a single seeded database | accepted |
| [ADR-0041](ADR-0041-end-to-end-tests-run-on-a-development-profile-by-default-and-a-production-build-profile-gates-client-changing-prds.md) | End-to-end tests run on a development profile by default, and a production-build profile gates client-changing PRDs | accepted |
| [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md) | Every end-to-end test is classified by whether it may run beside others | accepted |
| [ADR-0043](ADR-0043-playwright-e2e-testing-infrastructure.md) | Playwright E2E testing infrastructure | superseded |
| [ADR-0044](ADR-0044-retry-after-client-cooldown-handling.md) | Retry-After client cooldown handling | superseded |
| [ADR-0045](ADR-0045-no-end-to-end-test-shares-authentication-state-with-another.md) | No end-to-end test shares authentication state with another | accepted |
| [ADR-0051](ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) | Tenants are provisioned and destroyed by the product's own routes in tests | accepted |

### Backend

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0001](ADR-0001-optimistic-concurrency-is-checked-in-the-application.md) | Optimistic concurrency is checked in the application, not by the database | accepted |
| [ADR-0002](ADR-0002-endpoints-are-minimal-api-slices-with-their-own-models.md) | Endpoints are minimal API slices with their own request and response models | accepted |
| [ADR-0003](ADR-0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md) | The server is layered by responsibility with feature slices mirrored across the layers | accepted |
| [ADR-0004](ADR-0004-entity-framework-core-is-the-persistence-mechanism.md) | Entity Framework Core is the persistence mechanism | accepted |
| [ADR-0005](ADR-0005-postgresql-is-the-relational-datastore.md) | PostgreSQL is the relational datastore | accepted |
| [ADR-0006](ADR-0006-enumerations-are-stored-by-name.md) | Enumerations are stored by name | accepted |
| [ADR-0007](ADR-0007-uniqueness-is-enforced-by-the-database.md) | Uniqueness is enforced by the database | accepted |
| [ADR-0008](ADR-0008-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) | Cascades are restricted and deletion is an explicit, ordered sweep | accepted |
| [ADR-0009](ADR-0009-migrations-run-in-a-separate-process-before-the-api-starts.md) | Migrations run in a separate process before the API starts | accepted |
| [ADR-0010](ADR-0010-destructive-schema-changes-are-made-in-additive-steps.md) | Destructive schema changes are made in additive steps where possible | accepted |
| [ADR-0011](ADR-0011-services-access-data-through-feature-repositories.md) | Services read and write through feature repositories, never through the context | accepted |
| [ADR-0012](ADR-0012-every-failure-is-written-as-a-problem-details-document.md) | Every failure is written as a Problem Details document | accepted |
| [ADR-0013](ADR-0013-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md) | Request shape is validated at the edge and data-dependent rules in check chains | accepted |
| [ADR-0014](ADR-0014-only-guid-row-identifiers-are-exposed.md) | Only GUID row identifiers are exposed; integer keys stay internal | accepted |
| [ADR-0015](ADR-0015-expected-business-failures-are-returned-as-results.md) | Expected business failures are returned as results, not thrown as exceptions | accepted |
| [ADR-0020](ADR-0020-cross-origin-access-is-limited-to-configured-browser-origins.md) | Cross-origin access is limited to configured browser origins | accepted |
| [ADR-0021](ADR-0021-each-request-resolves-the-callers-permissions-from-the-database.md) | Each request resolves the caller's permissions from the database | accepted |
| [ADR-0022](ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md) | Access tokens are short-lived and backed by a server-side session | accepted |
| [ADR-0023](ADR-0023-passwords-are-hashed-with-the-framework-hasher.md) | Passwords are hashed with the ASP.NET Core hasher | accepted |
| [ADR-0024](ADR-0024-verification-and-password-reset-use-single-use-expiring-codes.md) | Verification and password reset use single-use, expiring codes | accepted |
| [ADR-0026](ADR-0026-background-work-runs-inside-the-api-process.md) | Background work runs inside the API process | accepted |
| [ADR-0027](ADR-0027-email-is-queued-in-process.md) | Email is queued in process | accepted |
| [ADR-0028](ADR-0028-email-bodies-are-rendered-from-razor-components.md) | Email bodies are rendered from Razor components | accepted |
| [ADR-0029](ADR-0029-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md) | Transactions are explicit and used only where writes must be one unit | accepted |
| [ADR-0030](ADR-0030-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md) | Tenant confinement is structural: query filters and site-scoped reads | accepted |
| [ADR-0034](ADR-0034-one-chained-rate-limit-policy-partitions-by-caller.md) | One chained rate-limit policy partitions by caller | accepted |
| [ADR-0035](ADR-0035-platform-administration-is-granted-by-deployment-configuration.md) | Platform administration is granted by deployment configuration, not by a role | accepted |
| [ADR-0036](ADR-0036-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) | Access tokens are rejected once their session or user version changes | accepted |
| [ADR-0037](ADR-0037-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md) | Tests are per-layer unit projects plus one hosted integration project | accepted |
| [ADR-0047](ADR-0047-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) | Every endpoint is authenticated unless it is explicitly anonymous | accepted |
| [ADR-0048](ADR-0048-validation-failures-answer-400-with-an-errors-map.md) | Validation failures answer 400 with an errors map, never 422 | accepted |
| [ADR-0049](ADR-0049-the-openapi-document-is-generated-at-build-time-and-committed.md) | The OpenAPI document is generated at build time and committed | accepted |
| [ADR-0050](ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md) | Request and failure logs record metadata, never a submitted value | accepted |
| [ADR-0052](ADR-0052-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md) | The API trusts the caller address and scheme from the ingress | accepted |
| [ADR-0054](ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md) | Server telemetry is instrumented with .NET's built-in APIs and exported through OpenTelemetry | proposed |
| [ADR-0055](ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md) | Services are registered by scanning each layer for its marker interface | accepted |

### Frontend

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0016](ADR-0016-the-client-is-organised-by-feature-over-shared-layers.md) | The client is organised by feature over shared API, concern and utility layers | accepted |
| [ADR-0017](ADR-0017-server-state-lives-in-a-tanstack-query-cache-with-explicit-invalidation.md) | Server state lives in a TanStack Query cache with explicit invalidation | accepted |
| [ADR-0018](ADR-0018-api-hooks-return-results-and-one-interceptor-normalises-failures.md) | API hooks return results and one interceptor normalises every failure | accepted |
| [ADR-0019](ADR-0019-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md) | The access token lives in memory, with proactive and reactive refresh | accepted |
| [ADR-0038](ADR-0038-the-pwa-precaches-the-application-shell-only-and-never-caches-api-responses.md) | The PWA precaches the application shell only and never caches API responses | accepted |
| [ADR-0039](ADR-0039-browser-storage-is-keyed-by-environment-and-user.md) | Browser storage is keyed by environment and user | accepted |
| [ADR-0046](ADR-0046-a-new-client-build-is-applied-by-enforced-update.md) | A new client build is applied by enforced update | accepted |
| [ADR-0053](ADR-0053-the-client-never-retries-a-rate-limited-request.md) | The client never retries a rate-limited request | accepted |

### Infrastructure

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0025](ADR-0025-secret-values-live-outside-the-repository.md) | Secret values live outside the repository, in a tracked template and a platform store | accepted |
| [ADR-0032](ADR-0032-production-runs-two-container-apps.md) | Production runs two container apps: nginx serves the SPA and the API is separate | accepted |
| [ADR-0033](ADR-0033-public-tls-terminates-at-the-container-apps-ingress.md) | Public TLS terminates at the Container Apps ingress | accepted |

## By status

### Proposed (1)

- [ADR-0054](ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md) — Server telemetry is instrumented with .NET's built-in APIs and exported through OpenTelemetry

### Accepted (52)

- [ADR-0001](ADR-0001-optimistic-concurrency-is-checked-in-the-application.md) — Optimistic concurrency is checked in the application, not by the database
- [ADR-0002](ADR-0002-endpoints-are-minimal-api-slices-with-their-own-models.md) — Endpoints are minimal API slices with their own request and response models
- [ADR-0003](ADR-0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md) — The server is layered by responsibility with feature slices mirrored across the layers
- [ADR-0004](ADR-0004-entity-framework-core-is-the-persistence-mechanism.md) — Entity Framework Core is the persistence mechanism
- [ADR-0005](ADR-0005-postgresql-is-the-relational-datastore.md) — PostgreSQL is the relational datastore
- [ADR-0006](ADR-0006-enumerations-are-stored-by-name.md) — Enumerations are stored by name
- [ADR-0007](ADR-0007-uniqueness-is-enforced-by-the-database.md) — Uniqueness is enforced by the database
- [ADR-0008](ADR-0008-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) — Cascades are restricted and deletion is an explicit, ordered sweep
- [ADR-0009](ADR-0009-migrations-run-in-a-separate-process-before-the-api-starts.md) — Migrations run in a separate process before the API starts
- [ADR-0010](ADR-0010-destructive-schema-changes-are-made-in-additive-steps.md) — Destructive schema changes are made in additive steps where possible
- [ADR-0011](ADR-0011-services-access-data-through-feature-repositories.md) — Services read and write through feature repositories, never through the context
- [ADR-0012](ADR-0012-every-failure-is-written-as-a-problem-details-document.md) — Every failure is written as a Problem Details document
- [ADR-0013](ADR-0013-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md) — Request shape is validated at the edge and data-dependent rules in check chains
- [ADR-0014](ADR-0014-only-guid-row-identifiers-are-exposed.md) — Only GUID row identifiers are exposed; integer keys stay internal
- [ADR-0015](ADR-0015-expected-business-failures-are-returned-as-results.md) — Expected business failures are returned as results, not thrown as exceptions
- [ADR-0016](ADR-0016-the-client-is-organised-by-feature-over-shared-layers.md) — The client is organised by feature over shared API, concern and utility layers
- [ADR-0017](ADR-0017-server-state-lives-in-a-tanstack-query-cache-with-explicit-invalidation.md) — Server state lives in a TanStack Query cache with explicit invalidation
- [ADR-0018](ADR-0018-api-hooks-return-results-and-one-interceptor-normalises-failures.md) — API hooks return results and one interceptor normalises every failure
- [ADR-0019](ADR-0019-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md) — The access token lives in memory, with proactive and reactive refresh
- [ADR-0020](ADR-0020-cross-origin-access-is-limited-to-configured-browser-origins.md) — Cross-origin access is limited to configured browser origins
- [ADR-0021](ADR-0021-each-request-resolves-the-callers-permissions-from-the-database.md) — Each request resolves the caller's permissions from the database
- [ADR-0022](ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md) — Access tokens are short-lived and backed by a server-side session
- [ADR-0023](ADR-0023-passwords-are-hashed-with-the-framework-hasher.md) — Passwords are hashed with the ASP.NET Core hasher
- [ADR-0024](ADR-0024-verification-and-password-reset-use-single-use-expiring-codes.md) — Verification and password reset use single-use, expiring codes
- [ADR-0025](ADR-0025-secret-values-live-outside-the-repository.md) — Secret values live outside the repository, in a tracked template and a platform store
- [ADR-0026](ADR-0026-background-work-runs-inside-the-api-process.md) — Background work runs inside the API process
- [ADR-0027](ADR-0027-email-is-queued-in-process.md) — Email is queued in process
- [ADR-0028](ADR-0028-email-bodies-are-rendered-from-razor-components.md) — Email bodies are rendered from Razor components
- [ADR-0029](ADR-0029-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md) — Transactions are explicit and used only where writes must be one unit
- [ADR-0030](ADR-0030-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md) — Tenant confinement is structural: query filters and site-scoped reads
- [ADR-0031](ADR-0031-builds-and-deployments-are-run-by-hand.md) — Builds, tests and deployments are run by hand; there is no continuous integration
- [ADR-0032](ADR-0032-production-runs-two-container-apps.md) — Production runs two container apps: nginx serves the SPA and the API is separate
- [ADR-0033](ADR-0033-public-tls-terminates-at-the-container-apps-ingress.md) — Public TLS terminates at the Container Apps ingress
- [ADR-0034](ADR-0034-one-chained-rate-limit-policy-partitions-by-caller.md) — One chained rate-limit policy partitions by caller
- [ADR-0035](ADR-0035-platform-administration-is-granted-by-deployment-configuration.md) — Platform administration is granted by deployment configuration, not by a role
- [ADR-0036](ADR-0036-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) — Access tokens are rejected once their session or user version changes
- [ADR-0037](ADR-0037-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md) — Tests are per-layer unit projects plus one hosted integration project
- [ADR-0038](ADR-0038-the-pwa-precaches-the-application-shell-only-and-never-caches-api-responses.md) — The PWA precaches the application shell only and never caches API responses
- [ADR-0039](ADR-0039-browser-storage-is-keyed-by-environment-and-user.md) — Browser storage is keyed by environment and user
- [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md) — End-to-end tests run on one shared stack against a single seeded database
- [ADR-0041](ADR-0041-end-to-end-tests-run-on-a-development-profile-by-default-and-a-production-build-profile-gates-client-changing-prds.md) — End-to-end tests run on a development profile by default, and a production-build profile gates client-changing PRDs
- [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md) — Every end-to-end test is classified by whether it may run beside others
- [ADR-0045](ADR-0045-no-end-to-end-test-shares-authentication-state-with-another.md) — No end-to-end test shares authentication state with another
- [ADR-0046](ADR-0046-a-new-client-build-is-applied-by-enforced-update.md) — A new client build is applied by enforced update
- [ADR-0047](ADR-0047-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) — Every endpoint is authenticated unless it is explicitly anonymous
- [ADR-0048](ADR-0048-validation-failures-answer-400-with-an-errors-map.md) — Validation failures answer 400 with an errors map, never 422
- [ADR-0049](ADR-0049-the-openapi-document-is-generated-at-build-time-and-committed.md) — The OpenAPI document is generated at build time and committed
- [ADR-0050](ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md) — Request and failure logs record metadata, never a submitted value
- [ADR-0051](ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) — Tenants are provisioned and destroyed by the product's own routes in tests
- [ADR-0052](ADR-0052-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md) — The API trusts the caller address and scheme from the ingress
- [ADR-0053](ADR-0053-the-client-never-retries-a-rate-limited-request.md) — The client never retries a rate-limited request
- [ADR-0055](ADR-0055-services-are-registered-by-scanning-each-layer-for-its-marker-interface.md) — Services are registered by scanning each layer for its marker interface

### Superseded (2)

- [ADR-0043](ADR-0043-playwright-e2e-testing-infrastructure.md) — Playwright E2E testing infrastructure
- [ADR-0044](ADR-0044-retry-after-client-cooldown-handling.md) — Retry-After client cooldown handling

## By PRD

| PRD | ADRs |
| --- | --- |
| PRD-0001 | [ADR-0022](ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md), [ADR-0039](ADR-0039-browser-storage-is-keyed-by-environment-and-user.md) |
| PRD-0002 | [ADR-0022](ADR-0022-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md), [ADR-0036](ADR-0036-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) |
| PRD-0012 | [ADR-0040](ADR-0040-end-to-end-tests-run-on-one-shared-stack.md), [ADR-0041](ADR-0041-end-to-end-tests-run-on-a-development-profile-by-default-and-a-production-build-profile-gates-client-changing-prds.md), [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md), [ADR-0043](ADR-0043-playwright-e2e-testing-infrastructure.md), [ADR-0045](ADR-0045-no-end-to-end-test-shares-authentication-state-with-another.md) |
| PRD-0014 | [ADR-0044](ADR-0044-retry-after-client-cooldown-handling.md) |
| PRD-0022 | [ADR-0008](ADR-0008-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) |
| PRD-0026 | [ADR-0021](ADR-0021-each-request-resolves-the-callers-permissions-from-the-database.md), [ADR-0030](ADR-0030-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md), [ADR-0047](ADR-0047-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) |
| PRD-0030 | [ADR-0012](ADR-0012-every-failure-is-written-as-a-problem-details-document.md), [ADR-0018](ADR-0018-api-hooks-return-results-and-one-interceptor-normalises-failures.md), [ADR-0048](ADR-0048-validation-failures-answer-400-with-an-errors-map.md), [ADR-0050](ADR-0050-request-and-failure-logs-record-metadata-never-a-submitted-value.md) |
| PRD-0031 | [ADR-0042](ADR-0042-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md), [ADR-0051](ADR-0051-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) |
| PRD-0035 | [ADR-0054](ADR-0054-server-telemetry-is-instrumented-with-dotnet-apis-and-exported-through-opentelemetry.md) |

<!-- END GENERATED INDEX -->
