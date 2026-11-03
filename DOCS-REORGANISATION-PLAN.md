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

| Criterion | Check |
| --- | --- |
| **No duplication** | Review every topic in the W2 list in `DOCS-WORK-LISTS.md`: each has one authoritative document, and other documents link to it or omit redundant detail. Use `docs-check -ReportFacts` only as a diagnostic; its broad word matches and limited file scope make it unsuitable as a completion gate. |
| **Nothing false** | Every W1 item has been confirmed against the code and fixed. No sample, version or role name contradicts what a reader would find in the repository. |
| **Easy to find** | From [Docs/README.md](Docs/README.md), any fact is two hops away, and a new document has an obvious row. An agent adding content finds a home for it without inventing one. |

## Rules

1. **One home per kind of document.** Placed by the table in [Docs/README.md](Docs/README.md).
2. **Never restate** what the code, a manifest, a project file or `contracts/openapi/v1.json` already
   says. Version numbers live in the manifests.
3. **Correct a document when you touch it.** No separate "documentation fix" pass; a file is fixed as it
   is moved or reworked.
4. **An ADR records a decision only when someone will ask "why is it like this?" and no surviving
   document answers it.** A convention is a rule document. A requirement is a PRD. A decision nobody
   will question does not need a record.
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

**Does:** apply the layout and the file mapping, split the three developer guides into an index plus
rule documents and worked examples, retire the stale plans, rename the folders. **Why:** this is the
"easy to find" criterion.

**Done when:** the files sit where [Docs/README.md](Docs/README.md) says they do, `docs-check` passes,
and the entry points in `README.md`, `AGENTS.md` and `copilot-instructions.md` all resolve.

### W4 — Records on demand

**Does:** write an ADR when the work above leaves a "why" that nothing answers. The inventory in
`DOCS-WORK-LISTS.md` W4 is a menu, not a backlog: no commitment, no target count. **Why:** the reasoning
is worth keeping, but only where it is asked for; writing eighty records up front adds eighty documents
that can go stale.

**Done when:** a reader following any accepted decision can find out why in one hop.

### Owner decisions

Three items need your call and are listed at the end of `DOCS-WORK-LISTS.md`; they block a link fix in
W3 and one setup-docs item in W1. Nothing else waits on them.

## Process lives in the skills, not in records

| Work | Procedure | Format |
| --- | --- | --- |
| Discovering a PRD | `.github/skills/prd-discovery/` | `.github/skills/prd-discovery/references/prd.template.md` |
| Delivering a PRD | `.github/skills/prd-delivery/` | the PRD template's Verification Evidence section |
| Writing an ADR | `.github/skills/adr/` | `.github/skills/adr/references/adr.template.md` |
| Amending a Complete PRD | `prd-delivery` close-out | the `AM-n` shape in the PRD template |
| Coding conventions | `.github/instructions/` | `Docs/standards/rule-template.md` |

`Docs/README.md` keeps a short "how we write docs" section, because a skill only loads for an agent: a
person editing a document still needs the placement rule in front of them.

## What this plan dropped from v1, and why

| Dropped | Why |
| --- | --- |
| A programme of about eighty backfilled ADRs | Serves "rationale lives once", not any of the three criteria. It is the largest single cost in v1 and adds eighty new documents that can drift. Replaced by W4. |
| Retrofitting twenty-nine old amendment notes | None of the three criteria needs history reorganised; the convention matters from now on, applied when a change actually contradicts a Complete PRD. |
| Four process ADRs and three standards ADRs | Process belongs in the skills and the two guides; the standards records restated the rule documents' own headings. Removed; the rule documents stand on their own. |
| Status and tier machinery on the decision inventory | It was planning overhead for a programme that no longer exists. The inventory is now a menu with a priority hint. |

## Sequencing

W1 first, because a correction to a file that is about to move is cheaper than one that has to be found
again. W2 and W3 then run together file by file, so each document is touched once. W4 follows, written
from what survives rather than from what we remember.

Each workstream ends with a stop for your review and an offered commit message. Agents never stage or
commit.

## Progress

| Workstream | State |
| --- | --- |
| Foundations (v1 Phase 1) | Done 2026-11-02: `docs-check` with 14 Pester tests, the ADR and PRD templates (with their skills), the docs map, the rule documents, `AGENTS.md`, and the skill changes. The seven meta records were removed when this plan replaced v1. |
| W1 Truth pass | Not started. |
| W2 Dedup pass | Not started. |
| W3 Move and split | Not started. |
| W4 Records on demand | Not started. |

## Verifying a change

```powershell
pwsh ./.github/scripts/docs-check/docs-check.ps1           # front matter, links, citations, IDs in code
Invoke-Pester ./.github/scripts/docs-check/docs-check.Tests.ps1
```

A documentation change is not ready to review until it passes.
