# Documentation Reorganisation Plan

**Status:** rewritten 2026-11-02. Replaces the six-phase plan.
**Baseline:** commit `9024e1d2`.
**Detail:** the per-file working lists are in [DOCS-WORK-LISTS.md](DOCS-WORK-LISTS.md), which is disposable.

This plan is deliberately short. The work is large, but the decisions behind it are few, and a plan
nobody reads cannot hold the work together.

## Goal

> The documents are all over the place and agents keep adding to them. Restructure them so there is no
> duplication, no false statement or example, and everything is easy to find.

That is the whole objective. Every workstream below serves one of those three things, and anything that
serves none of them is not in this plan.

## How we know it worked

Three checks, each run by a person in a few minutes:

| Criterion          | Check                                                                                                                                                                                                                                                                                               |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **No duplication** | Review every topic in the W2 list in `DOCS-WORK-LISTS.md`: each has one authoritative document, and other documents link to it or omit redundant detail. Use `docs-check -ReportFacts` only as a diagnostic; its broad word matches and limited file scope make it unsuitable as a completion gate. |
| **Nothing false**  | Every W1 item has been confirmed against the code and fixed. No sample, version or role name contradicts what a reader would find in the repository.                                                                                                                                                |
| **Easy to find**   | From [Docs/README.md](Docs/README.md), any fact is two hops away, and a new document has an obvious row. An agent adding content finds a home for it without inventing one.                                                                                                                         |

## Rules

1. **One home per kind of document.** Placed by the table in [Docs/README.md](Docs/README.md).
2. **Never restate** what the code, a manifest, a project file or `contracts/openapi/v1.json` already
   says. Version numbers live in the manifests.
3. **Correct a document when you touch it.** No separate "documentation fix" pass; a file is fixed as it
   is moved or reworked.
4. **Preserve the decisions that shaped the system, including decisions made before and during
   implementation.** Review the system from the point before it was written: what consequential
   alternatives were available, what was chosen, why, and what constraints or trade-offs followed?
   Backfill significant, evidenced decisions even when the implementation is stable. Code is the
   authority for current behavior; it is not, by itself, the history of why that behavior was chosen.
   Record one bounded decision per ADR. Group related ADRs into navigable decision families, but do not
   bundle independent choices into an umbrella ADR. Conventions belong in standards; product
   requirements belong in PRDs.
5. **A format lives with the skill that applies it.** The PRD and ADR templates sit in their skill's
   `references/` folder, next to the procedure that uses them; `Docs` links to them rather than holding
   a second copy.
6. **A private file stays private.** An ignored file is never staged, never copied into a tracked
   document and never edited as part of this work. Where a tracked document refers to one, the reference
   goes: a link only the owner can follow is worse than no link. If one genuinely must change, copy it
   aside, edit it, compare the two copies in a diff tool, and delete the copy — nothing enters git.

## Out of scope

- **Deployment as code.** The Azure deployment is click-ops, and the notes for it are private: they hold
  secrets and record one deployment choice that another reader may not share. Converting an
  already-deployed system to Bicep is worth doing, and is a separate PRD rather than part of this work.

## Where things go

The live table is in [Docs/README.md](Docs/README.md). The target shape:

```text
Docs/
  README.md                  the map, and the "where does X go?" table
  ARCHITECTURE.md            slim overview, links to records
  adr/                       decisions, written on demand
  prd/                       requirements (today Docs/Future/)
  standards/                 binding conventions: server/, client/, api/
  tutorials/  how-to/  reference/  explanation/  user-guide/
Source/Server/docs/          worked examples and recipes
Source/Client/pot-react/docs/
```

Folders are created when the content that belongs in them moves. Nothing is created up front.

## Workstreams

Each is independently useful: stopping after any one leaves the documentation better than it was.

### W1 — Truth pass

**Does:** verify and fix every claim in `DOCS-WORK-LISTS.md` W1, starting with the six that are wrong in
a security-relevant or code-corrupting way. **Why:** a false example is worse than no example, and it is
what makes a reader stop trusting the rest.

**Done when:** each item is marked confirmed or refuted against the code at that moment, fixed where
confirmed, and recorded in the commit message.

### W2 — Dedup pass

**Does:** for each topic in the W2 list, choose the surviving copy and delete the others, leaving a link.
Mostly ports, environment setup, the accrual definitions, the tenancy model, the two CORS copies and the
duplicated health-check blocks. **Why:** this is the "no duplication" criterion, and it is what makes a
single correction stick.

**Done when:** every W2 topic has one authoritative document, other mentions link to it or omit redundant
detail, and every deleted copy left a link or was genuinely redundant. Verify this against the W2 list;
`docs-check -ReportFacts` is diagnostic only.

### W3 — Move and split

**Does:** apply the layout and file mapping; retire stale plans; rename folders; and reduce the
developer guides to only content with a clear owner and ongoing value. The guides are migration
evidence, not authoritative descriptions of current code and not a mandate to create replacement
documents section by section. Point readers to code and tests for implementation patterns; retain
standards for binding rules, ADRs for evidenced decision rationale, and how-tos only where a sequence
adds durable value. Replace a guide with a concise index only if it improves navigation. **Why:** this
is the "easy to find" criterion without creating another layer of copied, drifting documentation.

**Done when:** the files sit where [Docs/README.md](Docs/README.md) says they do, `docs-check` passes,
and the entry points in `README.md`, `AGENTS.md` and `copilot-instructions.md` all resolve.

### W4 — Decision-history review and ADRs

**Does:** systematically review decisions made before and during implementation, using the W4
inventory, PRDs, dated records, commit history, and the developer guides as evidence. Confirm, split,
merge, or reject inventory topics; identify important gaps; and write an ADR for each consequential,
evidenced decision whose rationale should survive. Do not infer an undocumented historical rationale:
state what the evidence supports and what remains uncertain. The inventory is a review aid, not an ADR
numbering scheme or a fixed target count.

**Done when:** system-shaping decisions in the reviewed sources have either a concise ADR with evidence
and consequences, or a recorded reason why no ADR is appropriate. Related ADRs are easy to navigate as
families, while each record remains about one decision.

### Owner decisions

Two items need your call and are listed at the end of `DOCS-WORK-LISTS.md`; they block a link fix in
W3 and one setup-docs item in W1. Nothing else waits on them.

## Process lives in the skills, not in records

| Work                    | Procedure                       | Format                                                    |
| ----------------------- | ------------------------------- | --------------------------------------------------------- |
| Discovering a PRD       | `.github/skills/prd-discovery/` | `.github/skills/prd-discovery/references/prd.template.md` |
| Delivering a PRD        | `.github/skills/prd-delivery/`  | the PRD template's Verification Evidence section          |
| Writing an ADR          | `.github/skills/adr/`           | `.github/skills/adr/references/adr.template.md`           |
| Amending a Complete PRD | `prd-delivery` close-out        | the `AM-n` shape in the PRD template                      |
| Coding conventions      | `.github/instructions/`         | `Docs/standards/rule-template.md`                         |

`Docs/README.md` keeps a short "how we write docs" section, because a skill only loads for an agent: a
person editing a document still needs the placement rule in front of them.

## What this plan dropped from v1, and why

| Dropped                                             | Why                                                                                                                                                                                                                                                    |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A predetermined batch of about eighty ADRs          | A fixed count and preassigned record set would treat every inventory row as equally significant. Replaced by W4's evidence-led historical review: preserve consequential decisions, group related records for navigation, and do not invent rationale. |
| Retrofitting twenty-nine old amendment notes        | None of the three criteria needs history reorganised; the convention matters from now on, applied when a change actually contradicts a Complete PRD.                                                                                                   |
| Four process ADRs and three standards ADRs          | Process belongs in the skills and the two guides; the standards records restated the rule documents' own headings. Removed; the rule documents stand on their own.                                                                                     |
| Status and tier machinery on the decision inventory | It was planning overhead for a programme that no longer exists. The inventory is now a menu with a priority hint.                                                                                                                                      |

## Sequencing

W1 starts with demonstrably false, high-risk claims. Before W3 retires or splits a long-lived guide,
W4 reviews its decision material alongside the PRDs and other historical evidence; that way the guide
is treated as evidence to assess, not as a source to copy wholesale or discard wholesale. W2 and W3
then proceed file by file, using the resulting decision records as their rationale sources. Each
increment ends for owner review before the next begins.

Each workstream ends with a stop for your review and an offered commit message. Agents never stage or
commit.

## Progress

| Workstream                 | State                                                                                                                                                                                                                                    |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Foundations (v1 Phase 1)   | Done 2026-11-02: `docs-check` with 14 Pester tests, the ADR and PRD templates (with their skills), the docs map, the rule documents, `AGENTS.md`, and the skill changes. The seven meta records were removed when this plan replaced v1. |
| W1 Truth pass              | In progress: `AUTHENTICATION.md`, the server guide, `ARCHITECTURE.md`, the user-guide/FEATURES text, `IMPLEMENTATION.md` and the client guide corrected (W1 items 1–7, 9, 14 and the server-guide claims block; item 8 reduced to the DD-15 line); pending owner review. |
| W2 Dedup pass              | Not started.                                                                                                                                                                                                                             |
| W3 Move and split          | Not started.                                                                                                                                                                                                                             |
| W4 Decision-history review | In progress: criteria and initial candidate gaps updated in the working tree; pending owner review.                                                                                                                                      |

## Verifying a change

```powershell
pwsh ./.github/scripts/docs-check/docs-check.ps1           # front matter, links, citations, IDs in code
Invoke-Pester ./.github/scripts/docs-check/docs-check.Tests.ps1
```

A documentation change is not ready to review until it passes.
