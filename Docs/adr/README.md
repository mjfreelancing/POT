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
in the "Considered Options" of the ADR that replaced it. For a backfilled record, use the PRDs, dated
notes, commit history, guides and source as evidence. State when the record is reconstructed and do not
present an inference as established historical fact.

Review the system as it was being designed, not only as it behaves now. A decision can merit an ADR
even when it is old, stable or already encoded in tests. Current code establishes current behavior; it
does not necessarily explain why the system took that shape.

## How to create one

The template lives with the `adr` skill, which is what writes records:
[`.github/skills/adr/references/adr.template.md`](../../.github/skills/adr/references/adr.template.md).

1. Ask the agent to use the `adr` skill, or follow its procedure yourself.
2. Take the next number from the highest `id` in this folder. Numbers are never reused, and a withdrawn
   draft consumes its number.
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
Options (at least two, including the status quo for a backfilled record), Decision Outcome, Consequences
and Confirmation. Optional: Decision Drivers, Pros and Cons of the Options, More Information.

Front matter is eight lines: five required fields and three optional links. It is limited to flat
`key: value` pairs and simple lists, because `docs-check` parses it without a YAML library and rejects
anything else. A backfilled record is described in its Context section, not flagged in the front matter.

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

### Cross-cutting

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0001](0001-playwright-e2e-testing-infrastructure.md) | Playwright E2E testing infrastructure | accepted |
| [ADR-0002](0002-retry-after-client-cooldown-handling.md) | Retry-After client cooldown handling | proposed |

## By status

### Proposed (1)

- [ADR-0002](0002-retry-after-client-cooldown-handling.md) — Retry-After client cooldown handling

### Accepted (1)

- [ADR-0001](0001-playwright-e2e-testing-infrastructure.md) — Playwright E2E testing infrastructure

## By PRD

| PRD | ADRs |
| --- | --- |
| PRD-012 | [ADR-0001](0001-playwright-e2e-testing-infrastructure.md) |
| PRD-014 | [ADR-0002](0002-retry-after-client-cooldown-handling.md) |

<!-- END GENERATED INDEX -->
