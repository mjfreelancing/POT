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
   decision date only up to ADR-0049. On 2026-11-06, before the series reached `main`, it was renumbered
   once into date order, and PRD numbers were padded to four digits. Commit messages and branches from
   before that use the earlier numbers.
3. Copy the template to `NNNN-<kebab-case-title>.md` in this folder, keeping the four-digit number.
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

In ADR-0040 and ADR-0041 the heading's number carries `(legacy)` (`# ADR-0040 (legacy): …`). In every
other record up to ADR-0048, except ADR-0042, it carries `(backfilled)`: each was written after its
decision was in the code. The `id` and `title` fields carry no suffix. The suffix is the only marker, and
the body does not repeat it. Their `date` is when the decision was made: the date of the PRD the record
came from, otherwise the oldest commit in the `v0.1` history that introduced the decision. ADR-0042,
ADR-0049 and every later record carry no suffix.

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
| [ADR-0027](0027-builds-and-deployments-are-run-by-hand.md) | Builds, tests and deployments are run by hand; there is no continuous integration | accepted |

### Cross-cutting

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0037](0037-end-to-end-tests-run-on-one-shared-stack.md) | End-to-end tests run on one shared stack against a single seeded database | accepted |
| [ADR-0038](0038-end-to-end-tests-run-as-a-four-project-development-matrix-with-a-separately-run-production-build-profile.md) | End-to-end tests run as a four-project development matrix, with a separately run production-build profile | accepted |
| [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md) | Every end-to-end test is classified by whether it may run beside others | accepted |
| [ADR-0040](0040-playwright-e2e-testing-infrastructure.md) | Playwright E2E testing infrastructure | superseded |
| [ADR-0041](0041-retry-after-client-cooldown-handling.md) | Retry-After client cooldown handling | superseded |
| [ADR-0042](0042-no-end-to-end-test-shares-authentication-state-with-another.md) | No end-to-end test shares authentication state with another | accepted |
| [ADR-0047](0047-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) | Tenants are provisioned and destroyed by the product's own routes in tests | accepted |

### Backend

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0001](0001-optimistic-concurrency-is-checked-in-the-application.md) | Optimistic concurrency is checked in the application, not by the database | accepted |
| [ADR-0002](0002-endpoints-are-minimal-api-slices-with-their-own-models.md) | Endpoints are minimal API slices with their own request and response models | accepted |
| [ADR-0003](0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md) | The server is layered by responsibility with feature slices mirrored across the layers | accepted |
| [ADR-0004](0004-entity-framework-core-is-the-persistence-mechanism.md) | Entity Framework Core is the persistence mechanism | accepted |
| [ADR-0005](0005-postgresql-is-the-relational-datastore.md) | PostgreSQL is the relational datastore | accepted |
| [ADR-0006](0006-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md) | Enumerations are stored by name and uniqueness is enforced by the database | accepted |
| [ADR-0007](0007-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) | Cascades are restricted and deletion is an explicit, ordered sweep | accepted |
| [ADR-0008](0008-migrations-run-in-a-separate-process-before-the-api-starts.md) | Migrations run in a separate process before the API starts, in additive steps | accepted |
| [ADR-0009](0009-services-access-data-through-feature-repositories.md) | Services read and write through feature repositories, never through the context | accepted |
| [ADR-0010](0010-every-failure-is-written-as-a-problem-details-document.md) | Every failure is written as a Problem Details document | accepted |
| [ADR-0011](0011-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md) | Request shape is validated at the edge and data-dependent rules in check chains | accepted |
| [ADR-0012](0012-only-guid-row-identifiers-are-exposed.md) | Only GUID row identifiers are exposed; integer keys stay internal | accepted |
| [ADR-0013](0013-expected-business-failures-are-returned-as-results.md) | Expected business failures are returned as results, not thrown as exceptions | accepted |
| [ADR-0018](0018-cross-origin-access-is-limited-to-configured-browser-origins.md) | Cross-origin access is limited to configured browser origins | accepted |
| [ADR-0019](0019-each-request-resolves-the-callers-permissions-from-the-database.md) | Each request resolves the caller's permissions from the database | accepted |
| [ADR-0020](0020-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md) | Access tokens are short-lived and backed by a server-side session | accepted |
| [ADR-0021](0021-passwords-are-hashed-with-the-framework-hasher-and-reset-by-single-use-code.md) | Passwords are hashed with the ASP.NET Core hasher and reset by single-use code | accepted |
| [ADR-0023](0023-background-work-runs-inside-the-api-process.md) | Background work runs inside the API process under a single-replica assumption | accepted |
| [ADR-0024](0024-email-is-queued-in-process-and-rendered-from-razor-components.md) | Email is queued in process and rendered from Razor components | accepted |
| [ADR-0025](0025-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md) | Transactions are explicit and used only where writes must be one unit | accepted |
| [ADR-0026](0026-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md) | Tenant confinement is structural: query filters and site-scoped reads | accepted |
| [ADR-0030](0030-one-chained-rate-limit-policy-partitions-by-caller.md) | One chained rate-limit policy partitions by caller and never locks an account | accepted |
| [ADR-0032](0032-platform-administration-is-granted-by-deployment-configuration.md) | Platform administration is granted by deployment configuration, not by a role | accepted |
| [ADR-0033](0033-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) | Access tokens are rejected once their session or user version changes | accepted |
| [ADR-0034](0034-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md) | Tests are per-layer unit projects plus one hosted integration project | accepted |
| [ADR-0043](0043-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) | Every endpoint is authenticated unless it is explicitly anonymous | accepted |
| [ADR-0044](0044-validation-failures-answer-400-with-an-errors-map.md) | Validation failures answer 400 with an errors map, never 422 | accepted |
| [ADR-0045](0045-the-openapi-document-is-generated-at-build-time-and-committed.md) | The OpenAPI document is generated at build time and committed | accepted |
| [ADR-0046](0046-logs-record-request-metadata-and-never-submitted-values.md) | Logs record request metadata and never submitted values | accepted |
| [ADR-0048](0048-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md) | The API trusts the caller address and scheme from the ingress | accepted |

### Frontend

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0014](0014-the-client-is-organised-by-feature-over-shared-layers.md) | The client is organised by feature over shared API, concern and utility layers | accepted |
| [ADR-0015](0015-server-state-lives-in-a-query-cache-over-one-http-client.md) | Server state lives in TanStack Query over one configured HTTP client | accepted |
| [ADR-0016](0016-api-hooks-return-results-and-one-interceptor-normalises-failures.md) | API hooks return results and one interceptor normalises every failure | accepted |
| [ADR-0017](0017-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md) | The access token lives in memory, with proactive and reactive refresh | accepted |
| [ADR-0031](0031-a-rate-limited-caller-is-told-when-to-retry.md) | A rate-limited caller is told when to retry | proposed |
| [ADR-0035](0035-the-pwa-precaches-the-shell-only-and-enforces-updates.md) | The PWA precaches the application shell only and enforces updates | accepted |
| [ADR-0036](0036-browser-storage-is-keyed-by-environment-and-user.md) | Browser storage is keyed by environment and user, with a policy per setting | accepted |
| [ADR-0049](0049-the-client-never-retries-a-rate-limited-request.md) | The client never retries a rate-limited request | accepted |

### Infrastructure

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0022](0022-secret-values-live-outside-the-repository.md) | Secret values live outside the repository, in a tracked template and a platform store | accepted |
| [ADR-0028](0028-production-runs-two-container-apps.md) | Production runs two container apps: nginx serves the SPA and the API is separate | accepted |
| [ADR-0029](0029-public-tls-terminates-at-the-container-apps-ingress.md) | Public TLS terminates at the Container Apps ingress | accepted |

## By status

### Proposed (1)

- [ADR-0031](0031-a-rate-limited-caller-is-told-when-to-retry.md) — A rate-limited caller is told when to retry

### Accepted (46)

- [ADR-0001](0001-optimistic-concurrency-is-checked-in-the-application.md) — Optimistic concurrency is checked in the application, not by the database
- [ADR-0002](0002-endpoints-are-minimal-api-slices-with-their-own-models.md) — Endpoints are minimal API slices with their own request and response models
- [ADR-0003](0003-the-server-is-layered-with-feature-slices-mirrored-across-layers.md) — The server is layered by responsibility with feature slices mirrored across the layers
- [ADR-0004](0004-entity-framework-core-is-the-persistence-mechanism.md) — Entity Framework Core is the persistence mechanism
- [ADR-0005](0005-postgresql-is-the-relational-datastore.md) — PostgreSQL is the relational datastore
- [ADR-0006](0006-enumerations-are-stored-by-name-and-uniqueness-is-enforced-by-the-database.md) — Enumerations are stored by name and uniqueness is enforced by the database
- [ADR-0007](0007-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) — Cascades are restricted and deletion is an explicit, ordered sweep
- [ADR-0008](0008-migrations-run-in-a-separate-process-before-the-api-starts.md) — Migrations run in a separate process before the API starts, in additive steps
- [ADR-0009](0009-services-access-data-through-feature-repositories.md) — Services read and write through feature repositories, never through the context
- [ADR-0010](0010-every-failure-is-written-as-a-problem-details-document.md) — Every failure is written as a Problem Details document
- [ADR-0011](0011-request-shape-is-validated-at-the-edge-and-rules-in-check-chains.md) — Request shape is validated at the edge and data-dependent rules in check chains
- [ADR-0012](0012-only-guid-row-identifiers-are-exposed.md) — Only GUID row identifiers are exposed; integer keys stay internal
- [ADR-0013](0013-expected-business-failures-are-returned-as-results.md) — Expected business failures are returned as results, not thrown as exceptions
- [ADR-0014](0014-the-client-is-organised-by-feature-over-shared-layers.md) — The client is organised by feature over shared API, concern and utility layers
- [ADR-0015](0015-server-state-lives-in-a-query-cache-over-one-http-client.md) — Server state lives in TanStack Query over one configured HTTP client
- [ADR-0016](0016-api-hooks-return-results-and-one-interceptor-normalises-failures.md) — API hooks return results and one interceptor normalises every failure
- [ADR-0017](0017-the-access-token-lives-in-memory-with-proactive-and-reactive-refresh.md) — The access token lives in memory, with proactive and reactive refresh
- [ADR-0018](0018-cross-origin-access-is-limited-to-configured-browser-origins.md) — Cross-origin access is limited to configured browser origins
- [ADR-0019](0019-each-request-resolves-the-callers-permissions-from-the-database.md) — Each request resolves the caller's permissions from the database
- [ADR-0020](0020-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md) — Access tokens are short-lived and backed by a server-side session
- [ADR-0021](0021-passwords-are-hashed-with-the-framework-hasher-and-reset-by-single-use-code.md) — Passwords are hashed with the ASP.NET Core hasher and reset by single-use code
- [ADR-0022](0022-secret-values-live-outside-the-repository.md) — Secret values live outside the repository, in a tracked template and a platform store
- [ADR-0023](0023-background-work-runs-inside-the-api-process.md) — Background work runs inside the API process under a single-replica assumption
- [ADR-0024](0024-email-is-queued-in-process-and-rendered-from-razor-components.md) — Email is queued in process and rendered from Razor components
- [ADR-0025](0025-transactions-are-explicit-and-only-where-writes-must-be-one-unit.md) — Transactions are explicit and used only where writes must be one unit
- [ADR-0026](0026-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md) — Tenant confinement is structural: query filters and site-scoped reads
- [ADR-0027](0027-builds-and-deployments-are-run-by-hand.md) — Builds, tests and deployments are run by hand; there is no continuous integration
- [ADR-0028](0028-production-runs-two-container-apps.md) — Production runs two container apps: nginx serves the SPA and the API is separate
- [ADR-0029](0029-public-tls-terminates-at-the-container-apps-ingress.md) — Public TLS terminates at the Container Apps ingress
- [ADR-0030](0030-one-chained-rate-limit-policy-partitions-by-caller.md) — One chained rate-limit policy partitions by caller and never locks an account
- [ADR-0032](0032-platform-administration-is-granted-by-deployment-configuration.md) — Platform administration is granted by deployment configuration, not by a role
- [ADR-0033](0033-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) — Access tokens are rejected once their session or user version changes
- [ADR-0034](0034-tests-are-per-layer-unit-projects-plus-one-hosted-integration-project.md) — Tests are per-layer unit projects plus one hosted integration project
- [ADR-0035](0035-the-pwa-precaches-the-shell-only-and-enforces-updates.md) — The PWA precaches the application shell only and enforces updates
- [ADR-0036](0036-browser-storage-is-keyed-by-environment-and-user.md) — Browser storage is keyed by environment and user, with a policy per setting
- [ADR-0037](0037-end-to-end-tests-run-on-one-shared-stack.md) — End-to-end tests run on one shared stack against a single seeded database
- [ADR-0038](0038-end-to-end-tests-run-as-a-four-project-development-matrix-with-a-separately-run-production-build-profile.md) — End-to-end tests run as a four-project development matrix, with a separately run production-build profile
- [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md) — Every end-to-end test is classified by whether it may run beside others
- [ADR-0042](0042-no-end-to-end-test-shares-authentication-state-with-another.md) — No end-to-end test shares authentication state with another
- [ADR-0043](0043-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) — Every endpoint is authenticated unless it is explicitly anonymous
- [ADR-0044](0044-validation-failures-answer-400-with-an-errors-map.md) — Validation failures answer 400 with an errors map, never 422
- [ADR-0045](0045-the-openapi-document-is-generated-at-build-time-and-committed.md) — The OpenAPI document is generated at build time and committed
- [ADR-0046](0046-logs-record-request-metadata-and-never-submitted-values.md) — Logs record request metadata and never submitted values
- [ADR-0047](0047-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) — Tenants are provisioned and destroyed by the product's own routes in tests
- [ADR-0048](0048-the-api-trusts-the-caller-address-and-scheme-from-the-ingress.md) — The API trusts the caller address and scheme from the ingress
- [ADR-0049](0049-the-client-never-retries-a-rate-limited-request.md) — The client never retries a rate-limited request

### Superseded (2)

- [ADR-0040](0040-playwright-e2e-testing-infrastructure.md) — Playwright E2E testing infrastructure
- [ADR-0041](0041-retry-after-client-cooldown-handling.md) — Retry-After client cooldown handling

## By PRD

| PRD | ADRs |
| --- | --- |
| PRD-0001 | [ADR-0020](0020-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md), [ADR-0036](0036-browser-storage-is-keyed-by-environment-and-user.md) |
| PRD-0002 | [ADR-0020](0020-access-tokens-are-short-lived-and-backed-by-a-server-side-session.md), [ADR-0033](0033-access-tokens-are-rejected-once-their-session-or-user-version-changes.md) |
| PRD-0012 | [ADR-0037](0037-end-to-end-tests-run-on-one-shared-stack.md), [ADR-0038](0038-end-to-end-tests-run-as-a-four-project-development-matrix-with-a-separately-run-production-build-profile.md), [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md), [ADR-0040](0040-playwright-e2e-testing-infrastructure.md), [ADR-0042](0042-no-end-to-end-test-shares-authentication-state-with-another.md) |
| PRD-0014 | [ADR-0031](0031-a-rate-limited-caller-is-told-when-to-retry.md), [ADR-0041](0041-retry-after-client-cooldown-handling.md) |
| PRD-0022 | [ADR-0007](0007-cascades-are-restricted-and-deletion-is-an-explicit-ordered-sweep.md) |
| PRD-0026 | [ADR-0019](0019-each-request-resolves-the-callers-permissions-from-the-database.md), [ADR-0026](0026-tenant-confinement-is-structural-query-filters-and-site-scoped-reads.md), [ADR-0043](0043-every-endpoint-is-authenticated-unless-explicitly-anonymous.md) |
| PRD-0030 | [ADR-0010](0010-every-failure-is-written-as-a-problem-details-document.md), [ADR-0016](0016-api-hooks-return-results-and-one-interceptor-normalises-failures.md), [ADR-0044](0044-validation-failures-answer-400-with-an-errors-map.md), [ADR-0046](0046-logs-record-request-metadata-and-never-submitted-values.md) |
| PRD-0031 | [ADR-0039](0039-every-end-to-end-test-is-classified-by-whether-it-may-run-beside-others.md), [ADR-0047](0047-tenants-are-provisioned-and-destroyed-by-the-products-own-routes-in-tests.md) |

<!-- END GENERATED INDEX -->
