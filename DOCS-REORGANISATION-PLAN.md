# Documentation Reorganisation Plan

**Prepared:** 2026-10-28
**Status:** Draft for review, round 2. Nothing in the repository has been changed apart from this file and `DECISIONS-TO-DISCUSS.md`.
**How to review:** add comments inline anywhere, for example as a `> MS:` line under the text they refer to.
Every decision I need from you is in [section 2](#2-decisions-i-need-from-you) and has a `Your call:` line.
When you're done, ask me to read this file back. I'll work through your comments before anything is moved or written.

**Round 2 changes (2026-10-28):**

- D1–D12, the backlog and the Appendix C exceptions are answered. Where your answer changed or added something (D1, D6, D8, D10, D11, D12), an **Outcome** follows it. The rest of the plan has been updated to match.
- **New:** [D13](#d13--rule-documents-for-conventions-learning-from-clearweigh), which asks where coding conventions live. It came out of your D6 answer and the ClearWeigh ADR repo.
- **New:** [Appendix E](#appendix-e--developer-guide-classification), a section-by-section classification of the three developer guides (D6).
- **New file:** `DECISIONS-TO-DISCUSS.md` at the repo root, with one brief per D10 item and per code risk in section 11.

## Before executing this plan: re-baseline first

> **For the agent executing this plan:** this analysis is a snapshot. The source documents have changed since it was written and will change again before the plan is applied. Your first job is Phase 0b (below), not Phase 1.

- **Baseline:** commit `68ee3480` on `v0.1-prod-022-030`. This planning branch (`v0.1-prod-022-030-doc-update-plan`) was created from that commit and adds only this plan and `DECISIONS-TO-DISCUSS.md`. Every line number, count, quote and `path:line` reference in this plan, its appendices and the decisions file refers to `68ee3480`.
- **The documents have moved on:**
  - On 2026-10-29, `origin/v0.1-prod-022-030` was 10 commits ahead of the baseline.
  - More documentation updates are planned before this plan is applied.
  - Treat every reference here as a pointer to find the content, not as a guaranteed address.
- **Phase 0b, re-baseline.** Do this immediately before Phase 1, on the branch the plan will change:
  1. List what changed since the baseline:
     - `git diff --stat 68ee3480 <target> -- '*.md' .github Docs`;
     - plus any source file cited as evidence in section 8, Appendix E or `DECISIONS-TO-DISCUSS.md`.
  2. Read every changed or new document. Look for:
     - new decisions (backlog candidates, section 8);
     - new amendment notes, or PRDs newly overtaken by a later change (Appendices C and D);
     - new or changed rules (D13, Appendix E);
     - stale items in section 10 that have since been fixed;
     - code changes that alter the evidence or recommendation in a brief in `DECISIONS-TO-DISCUSS.md`.
  3. Check `Docs/Future/README.md` (or `Docs/prd/README.md`) for PRDs or ADRs added since the baseline, and add them to the inventory.
  4. Re-derive line references before using them.
  5. Write the findings as a dated "Re-baseline" addendum at the end of this plan. Then stop for the owner's review before starting Phase 1.
- **Known so far.** These 10 commits, as of 2026-10-29, change six documentation files. This list is a head start, not a substitute for step 2:
  - **E2E (`e2e.instructions.md`, `e2e/README.md`, `run-tests/SKILL.md`):**
    - The E2E run now starts and stops its own mail sink. This is progress on PRD-031, so it affects backlog row 0083 and the Appendix D entry for PRD-012.
    - E2E ports are now decided in one place, which touches rows 0082 and 0088.
    - The scripts are now tested with Pester 5 (`npm run e2e:scripts`, `e2e/scripts/tests/`). That is the precedent for D11's tests.
  - **`server.instructions.md`:** two new rules, both rule-document material under D13:
    - a marker interface declared beside the base type it marks is exempt from one-class-per-file;
    - lock fields use `System.Threading.Lock`.
  - **`aspnetcore-integration-tests.instructions.md` (+46 lines):** new integration-test rules, including authorization checked against every documented operation, the refusal order for a body that cannot be bound, and proof that a destructive action touches only what it names. Check these against row 0058 and the testing rule documents.
  - **`Docs/Future/TASKS-PRD-031.md`:** updated.

**How this was produced:** six read-only sub-agents ran in parallel:

- one per document family: general docs, the server developer guides, the client developer guides, and the PRDs together with the existing ADRs;
- one that worked only from source code and build configuration;
- one independent ADR-practice expert, given your goals but not my proposal, so its advice wasn't anchored on mine.

I spot-checked their key claims against the code: the forwarded headers, the concurrency token, the Postgres version, the money type, the two commits that carry decisions with no PRD, and the hard-coded ERD path. Line references are as of commit `68ee3480`.

---

## Contents

1. [Summary](#1-summary)
2. [Decisions I need from you](#2-decisions-i-need-from-you)
3. [What the review found](#3-what-the-review-found)
4. [PRD amendment history: my thoughts and the proposal](#4-prd-amendment-history-my-thoughts-and-the-proposal)
5. [The new ADR series](#5-the-new-adr-series)
6. [Target documentation structure](#6-target-documentation-structure)
7. [What happens to ARCHITECTURE.md](#7-what-happens-to-architecturemd)
8. [Proposed ADR backlog](#8-proposed-adr-backlog)
9. [Things that are not ADRs](#9-things-that-are-not-adrs)
10. [Content to correct during the migration](#10-content-to-correct-during-the-migration)
11. [Risks found in the code (for information)](#11-risks-found-in-the-code-for-information)
12. [Changes to skills, templates and agent instructions](#12-changes-to-skills-templates-and-agent-instructions)
13. [Migration phases](#13-migration-phases)
14. [Governance: keeping it from sprawling again](#14-governance-keeping-it-from-sprawling-again)
- [Appendix A — ADR template](#appendix-a--adr-template)
- [Appendix B — PRD amendment format](#appendix-b--prd-amendment-format)
- [Appendix C — Existing amendment notes to retrofit](#appendix-c--existing-amendment-notes-to-retrofit)
- [Appendix D — PRDs affected by later decisions that carry no note](#appendix-d--prds-affected-by-later-decisions-that-carry-no-note)
- [Appendix E — Developer guide classification](#appendix-e--developer-guide-classification)

---

## 1. Summary

1. **PRD amendments: yes, and it is worth doing properly.**
   - **Today:** later decisions are recorded in at least 11 formats under 10 different labels. Notes often sit far from the text they correct, header metadata is rarely bumped, and about 20 known impacts carry no note at all.
   - **The fix has three parts:**
     - an append-only `## Amendments` section at the end of each PRD;
     - a one-line `**Amended:**` header field so the reader sees it immediately;
     - in the PRD that *causes* a change, an `Effect on Earlier Documents` table, so the link exists in both directions.
   - **PRD body text is never edited.** See [section 4](#4-prd-amendment-history-my-thoughts-and-the-proposal).
2. **New ADR series:**
   - MADR-style ADRs with YAML front matter, in `Docs/adr/`, numbered `ADR-0001` upwards.
   - **The frontend/backend split:** one ADR per file, one number sequence, and an `area` field (`frontend`, `backend`, `cross-cutting`, `infrastructure`, `process`). The generated index groups ADRs by area. Agreed in [D1](#d1--adr-numbering-and-the-frontendbackend-split).
3. **The two existing ADRs are re-homed, not renumbered in place.**
   - ADR 012 (633 lines of mixed content) becomes about four focused ADRs. Its reference material moves to `e2e/README.md`.
   - ADR 014 becomes one Proposed ADR.
   - Stub files stay at the old paths so links in the frozen PRDs keep working.
4. **About 90 candidate ADRs** were found across PRDs, docs, developer guides and the code. I've consolidated them into a tiered backlog ([section 8](#8-proposed-adr-backlog)):
   - **Tier 1** (50) is the core set.
   - **Tier 3** is a candidate to fold into another ADR or drop.
   - Seven candidates are decisions **nobody has written a rationale for** (money as `double`, PostgreSQL 13, plaintext OTP codes, no CI, and so on). These, and the code risks in section 11, are discussed one at a time in `DECISIONS-TO-DISCUSS.md` ([D10](#d10--the-needs-a-decision-adrs)).
   - The ten small rows are folded into their neighbours (your call on the backlog).
5. **The rest of `Docs/` gets one home per document type.** Developer docs follow the Diátaxis types (tutorials, how-to, reference, explanation), with `adr/`, `prd/` and `user-guide/` as their own trees. Two smaller changes:
   - The 3,000- and 5,300-line `DEVELOPER.md` guides are separated into decisions (ADRs), rules (rule documents, [D13](#d13--rule-documents-for-conventions-learning-from-clearweigh)) and examples (which stay beside the code).
   - Definitions currently repeated in seven places (Accrued, Arrears, Committed, Available…) get one glossary.
6. **`ARCHITECTURE.md` is slimmed, not deleted.**
   - Today it holds a stack list, domain rules, test strategy and setup steps.
   - It becomes a 150–250 line map of the system, with no versions and no rationale, linking to the ADRs.
   - ADRs are a log of decisions, many superseded; they can't serve as a map ([D7](#d7--architecturemd-slim-or-delete)).
7. **Expect a lot of stale content.** The review found dozens of factual errors in non-PRD docs ([section 10](#10-content-to-correct-during-the-migration)). For example:
   - `AUTHENTICATION.md` says passwords use BCrypt; the code uses PBKDF2.
   - The user guide describes a "Site Owner" role that doesn't exist.
   - `ARCHITECTURE.md` describes production as Docker Compose; it is Azure Container Apps.
   - The server guide has a sample that echoes database error text to the caller.

   These get fixed as each file is reworked, not in a big bang.
8. **Sequencing:**
   1. Conventions and tooling first.
   2. Re-home the two existing ADRs.
   3. Write ADRs in themed batches.
   4. Retrofit the PRD amendments.
   5. Move and split files last, so each file is touched about once.
   6. A small local PowerShell `docs-check` script catches broken links and stale ADR citations, because there's no CI.

---

## 2. Decisions I need from you

Each decision has a recommendation. Where I recommend option A, the rest of this document assumes it.

### D1 — ADR numbering and the frontend/backend split

You asked for ADRs split into frontend and backend. Three ways to do it:

| Option | Shape | For | Against |
| --- | --- | --- | --- |
| **A. One sequence, `area` field** (recommended) | `Docs/adr/0042-….md`, `area: backend` in front matter; the generated index groups by area | IDs never change; one folder for agents to search; cross-cutting decisions have an obvious home; works with ADR tooling conventions | The split is a view, not physical folders |
| B. One sequence, area folders | `Docs/adr/backend/0042-….md`, `Docs/adr/frontend/0061-….md`, `Docs/adr/shared/…` | A visible physical split; numbers still unique | Reclassifying an ADR (it happens: client token handling vs server session model) moves the file and breaks links; agents must search more than one folder; a third "shared" folder is unavoidable |
| C. Separate sequences | `BE-001`, `FE-001`, `X-001` | Each side reads as its own series | Cross-cutting decisions have no natural sequence; reclassifying changes the ID; no tooling understands it |

Why the cross-cutting share matters: about 40% of the backlog is cross-cutting. That includes the Problem Details contract, auth and session tokens, tenancy, 429 handling, E2E, and the projection payload, which the client composes.

**Recommendation: A.** In the first pass I've also numbered the backlog in contiguous area blocks, so the initial series *reads* split even though the IDs are global. I use 4-digit IDs (`ADR-0001`) so the new series can't be confused with the legacy "ADR 012/014" names that frozen PRDs will keep mentioning.

> **Your call: A - I did mean split into different files, not folders**
>
> MS (round 2): Client is frontend and server is backend.

**Outcome:** A, one ADR per file.

- `area: frontend` covers `Source/Client`, and `area: backend` covers `Source/Server`.
- `cross-cutting`, `infrastructure` and `process` cover decisions that belong to neither side or to both. The API error contract is one: the server and the client must each honour it. Splitting such a decision into a frontend file and a backend file would create two records that drift apart.
- Confirmed in round 2: all five areas are used.

### D2 — Location and folder naming

- **Proposed home for ADRs:** `Docs/adr/`. They move out of `Docs/Future/`: that folder is defined as "implementation-planning focused", and ADRs are permanent records.
- **New folders:** lowercase kebab-case (`adr/`, `how-to/`, `user-guide/`).
- **Existing folders:** today they mix `Future`, `USER-GUIDE` and `Azure`.
- **Windows caveat:** case-only renames such as `USER-GUIDE` → `user-guide` need a two-step rename in git. You do the git side.

**Recommendation:** lowercase for everything new, and rename the existing folders in Phase 5.

> **Your call: Agree**

### D3 — How to retrofit the existing ad-hoc notes in PRDs

Twenty-nine notes exist across PRDs, ADRs and the index; they are listed in [Appendix C](#appendix-c--existing-amendment-notes-to-retrofit).

| Option | What happens |
| --- | --- |
| **A. Move verbatim** (recommended) | Each note's text moves unchanged into an `AM-n` entry in the new Amendments section. A note that warned about a specific paragraph is replaced in place by a one-line pointer (`> Amended — see AM-2`). Top-of-file banners go entirely and the `**Amended:**` header line replaces them. |
| B. Leave in place, add an index | Notes stay where they are; the Amendments section only links to them. No text moves, but the 11 formats remain and the section is a second copy of the same information. |

The notes are amendment text, not the PRD as approved, so moving them does not change the document's original content. I still want your explicit OK, given your "do not change any content" instruction.

**Recommendation: A.**

> **Your call: Agree**

### D4 — Add amendments for the impacts that were never noted

[Appendix D](#appendix-d--prds-affected-by-later-decisions-that-carry-no-note) lists about 20 places where a later PRD or commit contradicts an earlier document with no note. Examples:

- PRD-026 still says a configured platform administrator can never be disabled; PRD-022 relaxed that.
- PRD-012 says email E2E is deferred indefinitely; PRD-031 adds a mail sink.

**Recommendation:** add `AM-n` entries for each of these in Phase 4, each citing the cause.

> **Your call: Agree**

### D5 — Rename `Docs/Future/` to `Docs/prd/`

"Future" now holds 15 Complete PRDs, so the name misleads.

- **Same depth matters:** PRDs contain `../../Source/...` relative links, and a deeper path (for example `Docs/product/prds/`) would force edits to frozen PRDs.
- **Also affected:** the two PRD skills, the stage-plan template and `copilot-instructions.md`, which all hard-code `Docs/Future`. They change in the same commit.

**Recommendation:** rename to `Docs/prd/`, in Phase 5.

> **Your call: Agree**

### D6 — Where the developer guides live

`Source/Server/DEVELOPER.md` (5,309 lines) and `Source/Client/pot-react/DEVELOPER.md` (3,228 lines) are too long for people or agents to use.

| Option | Shape |
| --- | --- |
| **A. Split, keep beside code** (recommended) | `Source/Server/docs/*.md` (endpoints, services, repositories, EF Core, concerns, DI, …, each ≤ ~800 lines); `DEVELOPER.md` becomes a ≤150-line index. Same for the client. Reviewed with the code it describes, matched by the existing `applyTo` globs. |
| B. Move into `Docs/developer/server/…` | All docs in one tree, but further from the code and outside the instruction-file globs. |

**Rule either way:** `Docs/` holds product-wide and cross-cutting documentation; `Source/<component>/` holds internal developer reference for that component.

**Recommendation: A.**

> **Your call: Some, if not all, of those documents may be better as ADR's - thinks like naming conventions, project structure, validation, etc are all decisions that were made to ensure consistency across the code-base. Needs more discussion if you disagree or find there is a mixture of architecture content vs true content that should live with the code.**
>
> MS (round 2): Agree. Also look at the ClearWeigh ADR repo to see if it helps.

**Outcome:** each guide section mixes three layers, and each layer gets its own home:

| Layer | Example | Goes to |
| --- | --- | --- |
| **Decision:** why we do it | Why handlers are static; why failures are `EnrichedResult` values | An ADR. Most are already backlog rows (0042, 0045–0048, 0061–0064 and others). |
| **Rule:** the current binding convention | Handler location; validate → map → call → return; naming patterns | A rule document, per [D13](#d13--rule-documents-for-conventions-learning-from-clearweigh). The instruction files keep a condensed copy that cites it. |
| **Example:** worked code, recipes, catalogues, commands | Feature Implementation Recipe (~720 lines); Custom Components (~1,080 lines) | Stays beside the code in `Source/<component>/docs/`, linking to the ADR and the rule. |

Anything a tool enforces (`.editorconfig`, ESLint, Prettier, `tsconfig`) treats the config file as the source of truth; prose only points at it. [Appendix E](#appendix-e--developer-guide-classification) applies this split to every section of the three guides, so you can override it section by section.

### D7 — ARCHITECTURE.md: slim or delete

You expected it might be replaced entirely by ADRs.

- **Why ADRs alone can't replace it:** a newcomer or an agent can't rebuild the *current* shape of the system from 90 records, some superseded.
- **Why the path should stay:** the file is a convention both people and agents look for, and `copilot-instructions.md`, `README.md` and the guides reference it.

**Recommendation:** keep it at the same path as a 150–250 line overview ([section 7](#7-what-happens-to-architecturemd)). Its decision content goes to ADRs, and its domain explanation goes to `explanation/cashflow-model.md`.

> **Your call: Agree**

### D8 — Which superseded decisions get their own ADR

**Recommendation:** only decisions that *shipped* and left traces a reader will meet (migrations, frozen PRDs, old export formats). That gives three:

- persisted accrual with a dirty flag (PRD-003/009, superseded by PRD-023);
- refusals answering 422 (PRD-026, superseded by commit `bec69a48`);
- cache-first Docker builds (PRD-007, reversed after an incident).

Approaches that were tried and dropped *during* development go into the "Considered Options" section of the ADR that replaced them.

> **Your call: Agree. This also made me realise we probably need a document (or template) for the ADR creation, format, rules, etc. I imagine PRDs are much the same.**
>
> MS (round 2): Agree.

**Outcome:**

- **One human-readable guide per document type.** `Docs/adr/README.md` and `Docs/prd/README.md` each start with a hand-written "How this works" section covering:
  - when to write one;
  - how to create it;
  - the format;
  - the statuses and lifecycle;
  - amendments and supersession.

  The generated index sits below that section. ADR-0001 and ADR-0003 record *why* the rules exist; the READMEs say *how* to follow them.
- **One canonical template per type:** `Docs/adr/template.md` and `Docs/prd/template.md`. The PRD template moves out of `.github/skills/prd-discovery/references/`, and the skills link to the new location. This avoids keeping one copy for people and another for agents.
- The `adr` skill (section 12) follows the ADR README, as the PRD skills follow the PRD README.

### D9 — When a PRD-linked ADR becomes Accepted

**Recommendation:**

- An ADR raised by a PRD stays `proposed`, and freely editable, while the PRD is Planning or In Progress. This mirrors the existing "the PRD stays editable during delivery" rule.
- It flips to `accepted` when the PRD is marked Complete.
- It becomes `rejected` if the PRD is abandoned.
- Standalone ADRs are `accepted` when committed.
- **ADR status never tracks implementation progress.** ADR 012's "Accepted — implemented and validated" mixes the two; the PRD status already tracks implementation.

> **Your call: Agree**

### D10 — The "needs a decision" ADRs

Seven backlog items record choices that exist in the code with **no rationale anywhere**, and a few look like risks:

- money as `double`;
- PostgreSQL 13 locally (end of life November 2025) vs 17.6 in Azure;
- one-time codes stored in plaintext;
- hand-written client API types;
- DI by marker scanning, which contradicts `server.instructions.md`;
- no CI;
- the prodlike E2E gate (the docs disagree).

| Option | Meaning |
| --- | --- |
| **A. Record as-is, honestly** (recommended) | Write each as `accepted` describing what *is*, with "rationale: not recorded at the time" and the known consequences listed. Where you want a change, raise a PRD that later supersedes it. |
| B. Decide first | Settle each before writing its ADR. More correct, but it blocks the backlog on design work. |

> **Your call: Each item needs to be discussed and approved with the potential of needing to change code - such as if I decide to use decimal in place of double for money**
>
> MS (round 2): Agree. All risk items, such as forwarded headers, need to be discussed. A file at the repo root is good.

**Outcome:** neither A nor B. Each item is discussed and decided on its own.

- **The briefs:** `DECISIONS-TO-DISCUSS.md` at the repo root has one brief for each of the seven items above and for each risk in [section 11](#11-risks-found-in-the-code-for-information). Each brief gives:
  - what the code does today, with evidence;
  - the options, with their effort and whether a migration is needed;
  - my recommendation;
  - a `Your call:` line.
- **ADR status while undecided:** the ADR is not written; its backlog row stays `Dec`. Once you decide, I write the ADR:
  - **The decision keeps the code as it is:** the ADR is `accepted` and records your reasoning.
  - **The decision changes code:** a PRD is raised through `prd-discovery`, and the ADR stays `proposed` until that PRD is Complete (D9). Database changes include their migration, which you generate; I never do.
- **Nothing else waits.** The rest of the backlog goes ahead without these items.

### D11 — Index generation and the `docs-check` script

**Recommendation (revised in round 2):**

- A PowerShell 7 script at `.github/scripts/docs-check/docs-check.ps1`, with a `README.md`, matching the layout of `agent-env-tools/`. The original Node proposal is dropped, because every other script in the repo is PowerShell. It:
  - validates ADR front matter;
  - regenerates `Docs/adr/README.md`;
  - fails on broken relative links;
  - checks that instruction files cite only accepted ADRs;
  - greps `Source/**` for `PRD-`/`ADR-` references, which the current rule bans.
- Wired to a VS Code task, and optionally to a pre-commit hook.
- **Front matter:** PowerShell has no built-in YAML parser. Rather than add the `powershell-yaml` module as a dependency, the ADR front matter is limited to flat `key: value` pairs and simple lists. The script parses those itself and fails on anything else.
- **Tests:** Pester tests run against a fixtures folder of good and bad ADRs and links. The E2E scripts already use Pester 5 (`npm run e2e:scripts`, on `origin/v0.1-prod-022-030` since the baseline), so this adds no new tool.
- A hand-maintained index is the alternative. It will drift, like the current `Docs/Future/README.md` has.

> **Your call: A script is definitely required. Other scripts are ps1, why choose node when there are no others.**
>
> MS (round 2): Agree, and it can be tested if required.

**Outcome:** a PowerShell script, with Pester tests, as above.

### D12 — An `AGENTS.md` pointer

- **The gap:** the working rules and the future ADR rules live only in `.github/copilot-instructions.md` and the scoped instruction files. Copilot loads those, but agents that don't read Copilot's `.github/` conventions never see them. The repo has no `AGENTS.md`, the cross-tool convention for agent guidance.
- **The fix:** a short root `AGENTS.md` that points at `.github/copilot-instructions.md`, the instruction files and `Docs/adr/README.md`. It must not duplicate their rules, or the two will drift.
- **Copilot itself is unaffected** because it already reads `.github/`, so this is optional and only matters if you use other agents.
- **My role:** I have not created it; that is your call.

> **Your call: Create it**

**Outcome:** created in Phase 1, together with `Docs/adr/README.md` and the other files it points to, so that none of its links is broken.

### D13 — Rule documents for conventions (learning from ClearWeigh)

**Where this comes from.** Your D6 answer says conventions are decisions. The ClearWeigh ADR repo (`clearweigh-adr/adrs`) handles conventions differently from this plan:

- **Nine topic-level ADRs.** Examples: "Sensible engineering defaults", "Architecture", "API design" and "Data access".
- **Each ADR is a folder** containing:
  - a short `README.md` with the status, a table of dated events and the problem statement;
  - one or more **rule documents**, such as `naming-conventions.md`, `error-handling.md` and `project-structure.md`. These are 250–1,200 lines each and are binding.
- **Rule documents change by pull request**, with the reason written down. The ADR folder is not superseded when a single rule changes.
- **Each rule carries an enforcement tag:** `[error]`, `[warning]`, `[ci]` or `[review]`.
- **Each rule carries a provenance marker:** "Microsoft convention", "Project decision" or "Diverges from Microsoft". The marker tells a reader how cautiously to change it.
- **Precedence:** where a document and `.editorconfig` disagree, `.editorconfig` wins for anything a tool can check, and the document wins for everything else.
- **Brownfield rule:** existing working behaviour outranks a style rule.

**What fits POT and what doesn't:**

| ClearWeigh idea | Fit for POT |
| --- | --- |
| Rule documents as the home for conventions | **Good.** This is the gap your D6 answer points at. Today the rules are scattered across the developer guides and condensed into the instruction files, with nothing in between. |
| Enforcement tags | **Good,** adapted. POT has no CI, so the tags would be `[error]`, `[warning]`, `[test]` and `[review]`. `[test]` covers the architecture rules pinned by fixture tests (ADR-0058). |
| Provenance markers | **Good.** On the client the markers would name React, TanStack and shadcn conventions rather than Microsoft's. |
| Precedence and brownfield rules | **Good,** taken as they are. |
| Topic-level ADRs only (about 9–15 in all) | **Poor as the only form.** POT has many discrete decisions with real history: 422 → 400, accrual derived rather than stored, tokens bound to their session. D8 and D9 record those as single, unchangeable ADRs that can be superseded. Folding them into editable topic documents would lose that trail. |
| A status of "Completed" (built and in use) | **Conflicts with D9,** where you agreed that ADR status never tracks implementation. |
| A folder for every ADR | Unnecessary for 80 single-decision records. It is only useful where an ADR owns rule documents. |

**Options:**

| Option | Shape |
| --- | --- |
| **A. Hybrid** (recommended) | Single-decision ADRs stay as agreed: one file each, unchangeable once accepted. A few **standards ADRs** record the decision to keep a rule set, one each for server code, client code and the API contract. Each owns its rule documents in a top-level `Docs/standards/` folder: `server/`, `client/` and `api/`. Rules use ClearWeigh's tags, provenance markers, precedence and brownfield rules, and change by edit with a stated reason. A rule change that reverses a significant decision still needs its own ADR. Instruction files carry the condensed rule and cite the rule document. |
| B. Hybrid, with rules inside the ADR folder | As A, but each standards ADR becomes a folder holding its rule documents (`Docs/adr/00NN-server-standards/naming.md`). This matches ClearWeigh's layout, but `Docs/adr/` then mixes files and folders, and rule documents that change sit inside a folder of records that don't. |
| C. The ClearWeigh model throughout | About 12–15 topic ADR folders; single decisions become sections inside rule documents. This is consistent with ClearWeigh, but it loses the per-decision history and conflicts with D8 and D9. |

**Effect of A on the rest of the plan:**

- Section 5.3's "coding conventions stay in `.editorconfig` and the instruction files" becomes "conventions live in `Docs/standards/`".
- Section 9 shrinks. C# and TypeScript style become rule documents, and the component catalogues stay as examples beside the code.
- Backlog rows that are mostly convention become rules, and their ADR shrinks to the decision behind them or folds into a standards ADR. These are 0042, 0046, 0048, 0060 and 0076; Appendix E lists them per guide section. The backlog is renumbered once D13 is settled.
- The size budget for a rule document is about 800 lines, the same as a developer-guide topic file.

> **Your call:**

---

## 3. What the review found

### 3.1 The document estate

| Family | Files | Lines | Health |
| --- | --- | --- | --- |
| PRDs (`Docs/Future/PRD-*.md`) | 32 | ~8,800 | Three conform fully to the current template (022, 030, 031). PRD-001 to 011 are planning documents in a pre-template shape. Four header statuses disagree with the index (007, 009, 012, 027). PRD-032 is still a discovery document linked from the index as if it were the PRD. |
| Existing ADRs | 2 | 757 | Numbered after their PRD; mix decision, current-state description, config tables, how-to and design history; ADR-012 has a broken link to a non-existent instruction file. |
| `Docs/*.md` (architecture, auth, setup, features) | 7 | ~3,660 | Heavy duplication (see 3.2); `AUTHENTICATION.md` (2,217 lines) is part current and part describing the pre-AuthSession design. |
| User guide | 13 | ~1,560 | Accounts, Dashboard, Projections and Security are real; seven pages are stubs; a 745-line `_DOCUMENTATION_PLAN.md` is a stale work plan. |
| Developer guides | 5 | ~10,600 | Server `DEVELOPER.md` 5,309; client `DEVELOPER.md` 3,228; `IMPLEMENTATION.md` 825 (with encoding corruption); Docker `DEVELOPER.md` 840; `Data/README.md` 188. |
| Agent guidance (`.github/`) | 8 instructions + 13 skills | ~1,000 | Current and concise. It already carries many decisions as rules, with no "why". |

### 3.2 Duplication hot spots

| Topic | Number of places | Where |
| --- | --- | --- |
| Docker env-file setup | 7 | GETTING-STARTED, DOCKER-SETUP, Docker DEVELOPER, ARCHITECTURE, FIRST-TIME-SETUP, AUTHENTICATION (twice), LOCAL-SETUP |
| Ports | 9 | ARCHITECTURE, DOCKER-SETUP, Docker DEVELOPER (three places), copilot-instructions, docker.instructions, Data/README, LOCAL-SETUP, e2e.instructions |
| Accrued / Arrears / Committed / Available definitions | 7 | ARCHITECTURE, server DEVELOPER, IMPLEMENTATION, FEATURES, three user-guide pages |
| Tenancy model | 4 in server DEVELOPER alone | lines 156/182, 236, 1877, 4639 |
| CORS configuration | 2 full copies | server DEVELOPER 3413–3501 and 4950–5023 |
| Platform-admin bootstrap | 5 | AUTHENTICATION, FIRST-TIME-SETUP, GETTING-STARTED, LOCAL-SETUP, Docker DEVELOPER |
| Health-check verification blocks | 2 verbatim copies | DOCKER-SETUP 54–134 = 171–251 |
| Income vs expense renewal explanation | ~90% copy | IMPLEMENTATION 189–399 vs 402–604 |

### 3.3 Where decisions currently live

The review found **about 90 distinct architectural decisions**:

- **Embedded in PRDs:** about 40, in their "Decisions", "OD-nn", "D-n" and "Design Notes" sections.
- **In the developer guides and `AUTHENTICATION.md`:** about 30, often with good rationale.
- **Only as rules in `.github/instructions`:** about 10, with no rationale.
- **Only in the code:** about 10, the code being the sole evidence.

Two decisions were made **with no PRD at all**, so nothing can cite them today:

- the switch from 422 to 400, in commit `bec69a48` (2026-10-16);
- binding access tokens to their session, in commit `31c96313` (2026-10-17).

One existing note (ADR-012:362) consequently attributes the 422 change to PRD-030, which is wrong.

---

## 4. PRD amendment history: my thoughts and the proposal

### 4.1 Why the current approach fails

Your instinct is right. The audit found:

- **No single format.** There are at least 11:
  - bold-lead paragraphs;
  - blockquotes at the top of the document;
  - blockquotes at the top of a section;
  - indented blockquotes inside lists;
  - italic parentheticals inside the `Scope` header;
  - undated parentheticals;
  - "updated …" inside table cells;
  - an appended H2;
  - a dated H3 log;
  - an unlabelled paragraph;
  - clauses in the index summary column.
- **No searchable label.** "Supersession note", "Superseded contract note", "Problem Details note", "Snapshot note", "Note", "Needs rewriting", "Amended", "Updated", "Revision", "Supplementary Implementation Note".
- **Notes far from what they correct.** PRD-025's note sits in its header but corrects R5, R6, R8, the payload table and the Tests section, all of which still say `global`. PRD-026's notes sit *above* its Status block.
- **Metadata not bumped.** Last Updated stayed stale after amendment in PRD-012, PRD-014, PRD-018, PRD-019, PRD-026, PRD-027, ADR-012 and ADR-014.
- **One-way links.** PRD-023 and PRD-025 list what they annotated. PRD-022 and PRD-028 record nothing. PRD-022 relaxed a PRD-026 rule without naming it.
- **The convention is assumed but never defined.** PRD-023 OD-05, PRD-030 N2 and `prd-delivery` all say "per `Docs/Future` conventions", but no README, skill or template defines them. PRD-020 still lists it as an open question (DQ-5).
- **Status hides partial supersession.** PRD-003 and PRD-009 read "Complete" with nothing in the index flagging that most of PRD-009 no longer applies, except clauses in the Summary column. Those clauses break the README's own "one line" rule.

### 4.2 The proposal

**In every PRD that has been overtaken:**

1. A header line under the existing fields: `**Amended:** 2026-10-26 — 2 amendments, see [Amendments](#amendments)`. `Last Updated` keeps meaning "last approved".
2. A final `## Amendments` section, **append-only, oldest first**. Each entry is an `### AM-n — <title> (<date>)` subsection, not a table row: these notes run to several sentences, and prose in table cells is unreadable in raw Markdown. Each entry has:
   - **Type:**
     - `Superseded`: a rule was replaced.
     - `Obsoleted`: the thing described no longer exists.
     - `Corrected`: the document was wrong when written.
     - `Clarified`: no behaviour change.
   - **Affects:** requirement IDs (A1, B3) for template PRDs, and section anchors for older PRDs. This is what makes inline markers in the body unnecessary.
   - **Source:** the PRD, ADR or commit that made the change.
3. The original text is **never edited**. A pointer line (`> Amended — see AM-2`) is placed in the body only where a reader would otherwise act on a paragraph that is now wrong.

**In the PRD that causes a change:**

4. A new template section, `## Effect on Earlier Documents`, is filled in during discovery. It is a table: Document | What changes | Amendment. The causing PRD therefore lists its impacts *before* delivery, and `prd-delivery` writes the matching `AM-n` entries when the PRD closes. That gives a link in each direction.

**Elsewhere:**

5. **Architectural supersessions** are also recorded as an ADR with `supersedes`/`amends`. The PRD amendment then cites the ADR. Product-rule changes (for example "a platform admin may now be removed while another exists") need only the amendment.
6. **The index** (`Docs/prd/README.md`) gains a narrow `Amended` column holding the latest amendment date. Summary cells go back to one line describing what the feature *is*.
7. **Scope:** amendments apply to **Complete** (or abandoned) PRDs. A PRD that has not been delivered absorbs changes directly when it is next revised. PRD-027 ("Blocked, needs rewriting") is the example: its existing note is folded in during its rewrite, not retrofitted.

The format is shown in [Appendix B](#appendix-b--prd-amendment-format).

### 4.3 Retrofitting the existing PRDs

- **Content:** none of the PRDs' original content changes. The work is:
  - adding the Amendments section and header line;
  - moving the existing notes into it, if [D3](#d3--how-to-retrofit-the-existing-ad-hoc-notes-in-prds) is A;
  - adding entries for the gaps, if [D4](#d4--add-amendments-for-the-impacts-that-were-never-noted) is yes.
- **Header disagreements:** headers whose status disagrees with the index (PRD-007 "Planned", PRD-009 "Implemented", PRD-028:55 "implementation has not started") are recorded as `Corrected` amendments, not edited.
- **Older PRDs:** PRD-001 to 021 are *not* reshaped into the new template. Their structure is historical record. They only gain the Amendments section where needed.

---

## 5. The new ADR series

### 5.1 Template

The template is **MADR 4, customised**: Nygard's format is too thin for backfilled records, and MADR's "Confirmation" section is the natural hook for tests and instruction files. The full skeleton is in [Appendix A](#appendix-a--adr-template).

**Required sections:**

| Section | Rule |
| --- | --- |
| Summary | One sentence: "In the context of…, facing…, we decided…, to achieve…, accepting…" |
| Context and Problem Statement | — |
| Considered Options | At least two, including the status quo for backfills |
| Decision Outcome | — |
| Consequences | Good and bad |
| Confirmation | The test, analyzer, instruction file or review rule that enforces the decision |

**Optional sections:** Decision Drivers, Pros and Cons of the Options, More Information.

**Front matter:**

| Field | Values |
| --- | --- |
| `id`, `title` | — |
| `status` | `proposed`, `accepted`, `rejected`, `deprecated` or `superseded` |
| `date` | When the decision was made |
| `recorded` | When the file was written |
| `provenance` | `contemporaneous` or `retroactive` |
| `evidence` | Required when retroactive |
| `area` | — |
| `tags` | — |
| `supersedes`, `superseded-by`, `amends`, `amended-by` | — |
| `related-prds`, `related-adrs` | — |

**Length:** target 40–120 lines, with a ceiling of about 150. A longer draft is several decisions (split it) or a design document (move the content to reference or explanation).

### 5.2 Lifecycle and immutability

- **Lifecycle:** `proposed` → `accepted` → `deprecated` | `superseded`, or `proposed` → `rejected`.
- **When it becomes `accepted`:** see [D9](#d9--when-a-prd-linked-adr-becomes-accepted).
- **Edits allowed once accepted:**
  - typo and link fixes;
  - the `status`, `superseded-by` and `amended-by` fields;
  - a dated clarification line that doesn't change the decision.
- **Amend or supersede:**
  - *Amend*, via a new ADR with `amends:`, when the decision mostly still holds and something narrower changes.
  - *Supersede* when the decision statement is no longer true.
  - Never rewrite an accepted decision in place.
- **Backfilled ADRs:**
  - `provenance: retroactive`.
  - `date` is the best evidence of when the decision was made: the PRD completion date, a dated note, or the commit date.
  - `recorded` is the day the file is written.
  - `evidence` links PRD sections and source files. Because PRDs are frozen, deep links into them stay stable.

### 5.3 Relationship to PRDs

| Document | Records |
| --- | --- |
| PRD | What and why for one feature: requirements, scope, actors, product rules, the test plan. Frozen at Complete. |
| ADR | One significant technical decision that outlives the feature, with the alternatives and consequences. |

**Significance test.** Raise an ADR if any of these is true:

- it constrains features beyond this PRD;
- it is costly to reverse;
- it crosses components or defines the client–server contract;
- someone will later ask "why is it like this?".

**What does not get an ADR:**

- Product rules stay in PRDs.
- Coding conventions are rules, not decisions. Their home depends on [D13](#d13--rule-documents-for-conventions-learning-from-clearweigh); under option A it is `Docs/standards/`. See also [section 9](#9-things-that-are-not-adrs).

**Linking:**

- **ADR → PRD:** the ADR's front matter lists `related-prds`.
- **PRD → ADR (new PRDs):** the PRD's "Decisions Carried Into This Document" table gains an `ADR` column.
- **PRD → ADR (existing PRDs, which are not edited):** the generated ADR index includes an "ADRs by PRD" view.

### 5.4 The two existing ADRs

| Legacy | Becomes | Non-decision content goes to |
| --- | --- | --- |
| `Docs/Future/ADR-012-playwright-e2e-testing-infrastructure.md` (633 lines) | ADR-0079 to 0082 (shared stack and seeded database; test isolation classes; fresh login per test; execution policy). The rejected designs in Appendix A become their Considered Options. | Config values, ports, seeded identities and directory tree go to `e2e/README.md` (with no copied config tables that drift). Operational notes in §13 go to a troubleshooting section there. How-to content goes to `e2e/AUTHORING.md`. |
| `Docs/Future/ADR-014-retry-after-client-cooldown-handling.md` (124 lines) | ADR-0072, `proposed`, decisions D-1 to D-5 | The open questions stay in PRD-014. "Where changes would land" goes nowhere durable (it belongs in the TASKS plan). |

At each old path a **tombstone stub** of about five lines says "moved to ADR-00xx…". PRD-012, PRD-014 and the index keep working links without editing frozen PRDs.

---

## 6. Target documentation structure

### 6.1 Layout

```text
README.md                          repo overview; links to Docs/README.md
Docs/
  README.md                        docs map + "where does X go?" table (the anti-sprawl contract)
  ARCHITECTURE.md                  slim overview, links to ADRs (section 7)
  adr/
    README.md                      "How this works" (D8) + generated index: by area, by status, by PRD
    template.md
    0001-record-architecture-decisions.md …
  prd/                             renamed from Future/ (same depth, D5)
    README.md                      "How this works" (D8) + PRD index (one-line summaries + Amended column)
    template.md                    moved from the prd-discovery skill (D8)
    PRD-NNN-*.md, TASKS-PRD-NNN.md
  standards/                       rule documents, if D13 option A is chosen
    server/  client/  api/
  tutorials/
    getting-started.md             single entry tutorial (absorbs DOCKER-SETUP's quick start)
    first-time-setup.md
    images/first-time-setup/…
  how-to/
    run-locally-without-docker.md
    run-the-docker-stack.md
    configure-platform-administrators.md
    troubleshoot-auth-cookies-and-cors.md
    deploy-to-azure.md             replaces the referenced-but-missing Azure-Deployment-Notes.md
  reference/
    configuration.md               every env var / options section, once
    ports-and-environments.md      every port and stack, once
    permissions.md                 the real permission list and roles
    auth-and-sessions.md           token lifetimes, claims, cookie attributes, auth endpoints
    glossary.md                    Accrued, Arrears, Committed, Available, Daily Need, Reserved …
    diagrams/                      pot_erd.d2/.png, Azure .mmd/.png + generate script
  explanation/
    cashflow-model.md              the obligation-first model with worked examples
    authentication.md              signup, login, refresh, logout, reset flows (concepts, no code dumps)
    multi-tenancy.md
  user-guide/                      renamed from USER-GUIDE/
Source/Server/DEVELOPER.md         ≤150-line index (D6)
Source/Server/docs/*.md            worked examples and recipes only (D6), each ≤ ~800 lines
Source/Client/pot-react/DEVELOPER.md   index
Source/Client/pot-react/docs/*.md      worked examples and component catalogue only (D6)
Source/Client/pot-react/e2e/README.md, AUTHORING.md   stay (reference + how-to)
Source/Docker/DEVELOPER.md         stays as the Docker reference
Data/README.md                     stays
```

### 6.2 Where does X go?

This table becomes the core of `Docs/README.md`.

| If you are writing… | It goes in |
| --- | --- |
| A significant technical decision, and why | `Docs/adr/` |
| What a feature must do, and for whom | `Docs/prd/` |
| A later change to a Complete PRD | that PRD's `## Amendments` |
| Steps a newcomer follows once, start to finish | `Docs/tutorials/` |
| Steps to achieve one task | `Docs/how-to/` (product-wide) or `Source/<component>/docs/` (component-internal) |
| Facts to look up (ports, config keys, permissions, claims) | `Docs/reference/` |
| How a concept works as a whole | `Docs/explanation/` |
| A binding coding convention (naming, structure, validation, …) | `Docs/standards/` (D13 option A) |
| How to write code in one component (worked examples, recipes, catalogues) | `Source/<component>/docs/` |
| What an end user sees and does | `Docs/user-guide/` |
| A rule an agent must follow | `.github/instructions/` (the condensed rule plus a citation of its ADR or rule document, no rationale) |
| A term with a precise meaning | `Docs/reference/glossary.md` |

Two rules come with it:

- **Never restate** what the code, `package.json`, the csproj files or `contracts/openapi/v1.json` already say. Version numbers belong in package manifests, not prose.
- **Rationale lives once**, in the ADR. Everything else links to it.

### 6.3 Mapping of existing files

| Current | Action |
| --- | --- |
| `README.md` | Keep. Add links to the docs map and the ADR index; the setup docs are currently missing from its list. |
| `Docs/ARCHITECTURE.md` | Slim (section 7). Cashflow sections go to `explanation/cashflow-model.md` and ADRs 0008–0015. The test split goes to ADR-0058. Setup steps are deleted (they duplicate the tutorial). |
| `Docs/AUTHENTICATION.md` | Split four ways (the content is listed below the table). Drop the large code snapshots: several describe the removed `User.RefreshToken` design. |
| `Docs/GETTING-STARTED.md` | Becomes `tutorials/getting-started.md`, the single entry point. |
| `Docs/DOCKER-SETUP.md` | Its quick start merges into the tutorial; the rest goes to `how-to/run-the-docker-stack.md`. The duplicated verification blocks are removed. |
| `Docs/LOCAL-SETUP.md` | Becomes `how-to/run-locally-without-docker.md`. Option B is replaced with the `Data/Local Docker` stack. |
| `Docs/FIRST-TIME-SETUP.md` (+ `Docs/images/first-time-setup/`) | Becomes `tutorials/first-time-setup.md`. It becomes the only place for bootstrapping the first platform admin. |
| `Docs/FEATURES.md` | Merges into `user-guide/README.md` (a third copy of the feature list). |
| `Docs/USER-GUIDE/*` | Moves to `user-guide/`. Projections.md's client persistence and fetch-window spec moves to ADRs and reference. |
| `Docs/USER-GUIDE/_DOCUMENTATION_PLAN.md` | Retire after harvesting anything still valid. It is a stale work plan, wrong in at least six places. |
| `Docs/Future/PRD-*.md`, `README.md`, `TASKS-*.md` | Move to `Docs/prd/` (D5). Contents unchanged apart from the amendments. |
| `Docs/Future/ADR-012…`, `ADR-014…` | Re-homed (5.4). Tombstones are left behind. |
| `Docs/pot_erd.d2` / `.png` | Move to `reference/diagrams/`. **Caveat:** `Pot.Data.Migrations/App.cs:61` hard-codes `..\..\..\..\..\..\Docs\pot_erd.d2`, so moving it is a one-line code change (your approval), or the file stays where it is. |
| `Docs/Azure/*` | Moves to `reference/diagrams/`. `Docs/pot-azure-diagram.png` is a *different, older* render of the same diagram; delete it. |
| `Source/Server/DEVELOPER.md` | Split (D6). "Key Architectural Principles" (148–197) becomes links to ADRs. The second CORS copy is deleted. |
| `Source/Server/IMPLEMENTATION.md` | Its domain explanation goes to `explanation/cashflow-model.md`, and its calculator detail to `Source/Server/docs/calculations.md`. Fix the encoding. Income and expense renewal collapse into one section with a differences table. |
| `Source/Client/pot-react/DEVELOPER.md` | Split (D6). Regenerate the stale tree and table of contents. |
| `Source/Docker/DEVELOPER.md` | Keep. Fix the three dead links to `Azure-Deployment-Notes.md`. |
| `Data/README.md` | Keep. Fix the port claims at lines 96 and 182. |

`Docs/AUTHENTICATION.md` splits into:

- about ten ADRs (0030–0039, 0071);
- `explanation/authentication.md` (flows);
- `reference/auth-and-sessions.md`;
- two how-tos (platform admins; cookies and CORS);
- the user-facing parts, which move to `user-guide/Security.md`.

---

## 7. What happens to ARCHITECTURE.md

It stays at `Docs/ARCHITECTURE.md`, rewritten as a 150–250 line arc42/C4-lite overview:

1. **Purpose and quality goals.** Projection-first, obligation-first, deterministic server-side computation, tenant isolation.
2. **Context and container diagram.** Mermaid, rendered with the existing `mmd2png` skill: browser/PWA → nginx → API → PostgreSQL, SMTP, Azure.
3. **Building blocks.** Server layers, client feature modules, test projects, one paragraph each.
4. **Cross-cutting concepts.** One paragraph each with ADR links: tenancy, authentication and sessions, the error contract, concurrency, rate limiting, health, time and money.
5. **Deployment view.** Local stacks and Azure, linking `reference/ports-and-environments.md`.
6. **A link to the ADR index.**

It must not contain:

- **Version numbers.** They already drift: ADR-012 says Vite 6 while the code is on Vite 8.
- **Rationale.** That lives in the ADRs.
- **Setup steps.** Those live in the tutorial.
- **Domain formulae.** Those live in `explanation/cashflow-model.md` and the glossary.

---

## 8. Proposed ADR backlog

**Reading the table:**

- **Numbers** are provisional and grouped by area for the first pass. A dropped row consumes no number; the final number is assigned when the ADR is written.
- **Status values:**
  - `Acc`: accepted, decided and current.
  - `Sup`: superseded (written for history, per D8).
  - `Prop`: proposed, its PRD not complete.
  - `Dec`: needs your call (D10).
  - `Ver`: verify against the code before writing, because the docs disagree.
- **Tier values:**
  - `1`: write first.
  - `2`: write in the main pass.
  - `3`: fold into a neighbour or drop.

### Process

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0001 | Record architecture decisions in one numbered ADR series | Acc (new) | this plan | 1 |
| 0002 | Give every kind of document exactly one home | Acc (new) | this plan §6 | 1 |
| 0003 | Freeze PRDs at Complete; record later change as amendments | Acc (new) | PRD-023 OD-05, PRD-030 N2 (implicit today) | 1 |
| 0004 | Deliver PRDs through the discovery/delivery skills; agents never stage or commit; no PRD/ADR IDs in code | Acc | prd skills; copilot-instructions:7–18 | 1 |
| 0005 | Layer agent guidance (always-on, glob-scoped, skills); instruction files cite ADRs | Acc | .github/README; copilot-instructions | 2 |
| 0006 | CRLF line endings everywhere except shell scripts | Acc | .gitattributes; .editorconfig; .prettierrc; OpenAPI normaliser | 3 |

### Product and domain (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0007 | POT forecasts from schedules; it is not a transaction register (manual balances, no bank feeds) | Acc | README:9–50; copilot-instructions:3; PRD-024:119 | 1 |
| 0008 | Obligation-first, cash-basis model with rolling forecast windows | Acc | ARCHITECTURE:146–189 | 1 |
| 0009 | Persist account accrual and recalculate on a dirty flag | Sup by 0010 | PRD-003; PRD-009 | 2 |
| 0010 | Derive accrual on read from the schedule; never persist a computed value | Acc | PRD-023 | 1 |
| 0011 | Two accrual metrics (stable daily need; dynamic operational), gated by per-expense policy `Automatic`/`None` | Acc | PRD-003; ARCHITECTURE:120–132; IMPLEMENTATION:123–135 | 1 |
| 0012 | Obligation components: Accrued + Arrears = Committed; Available = Balance − Reserved − Committed | Acc | PRD-023 OD-17; ARCHITECTURE:138–144 | 1 |
| 0013 | `NextDue` is the user-acknowledged schedule cursor; due-day timing, renewal modes and calendar arithmetic (incl. `EndOfMonth`) | Acc | PRD-023:32, 228; IMPLEMENTATION:65–398; FrequencyExtensions.cs | 1 |
| 0014 | Projections are a day-by-day in-memory simulation over immutable cursors; nothing is persisted | Acc | PRD-023:186–204; IMPLEMENTATION:608–823 | 2 |
| 0015 | The projection API publishes components; the client composes balances and totals | Acc | PRD-025; PRD-028 | 1 |
| 0016 | One business time zone (Australia/Sydney) through `ITimeProvider`; `DateOnly` schedules, UTC timestamps | Acc (interim; PRD-018 proposes superseding) | Pot.App/AppContext.cs; ITimeProvider | 1 |
| 0017 | Money is `double`, rounded to 2 dp away from zero; AUD and en-AU assumed | Dec | pot_erd.d2; ExpenseEntity.cs:25; moneyUtils.ts | 1 |
| 0018 | Export packages are versioned; import accepts only the current version and never deletes | Acc | PRD-021:91–173; PRD-023 OD-02; MetadataV1–V4 readers | 2 |
| 0019 | A settlement ledger, if built, is history only and never feeds a calculation | Prop | PRD-024 | 3 |

### API contract (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0020 | Client and server release together; breaking API changes need no compatibility window | Acc | PRD-021:169; PRD-025:261; PRD-028:73; PRD-030:157 | 1 |
| 0021 | Expose only GUID `RowId`s; integer keys stay internal | Acc | server.instructions:17; EntityBase.cs | 1 |
| 0022 | Optimistic concurrency via an app-managed `Etag` in the body; a stale tag answers 409 | Acc + Ver | server DEVELOPER:4744–4823; PRD-030:137; CheckHasSameEtag (not a database concurrency token, see §11) | 1 |
| 0023 | Every failure is RFC 9457 Problem Details, written regardless of `Accept` | Acc | PRD-030; server DEVELOPER:3823–3906 | 1 |
| 0024 | Refusals answer 422 Unprocessable Entity | Sup by 0025 | PRD-026:428–433 | 2 |
| 0025 | Validation failures and refused business rules answer 400 with an `errors` map | Acc | commit `bec69a48` (2026-10-16); PRD-030 | 1 |
| 0026 | 409 conflicts are distinguished by problem `type` URN | Acc | PRD-030; server DEVELOPER:956–966 | 2 |
| 0027 | A correlation ID flows from the client header into logs and every problem body | Acc | server DEVELOPER:3706–3729; CorrelationIdMiddleware; apiHelpers.ts | 2 |
| 0028 | The OpenAPI document is generated at build time and committed as the contract | Acc | server.instructions:21–28; Pot.AspNetCore.csproj | 1 |
| 0029 | Client API types are hand-written; not generated from, or validated against, the contract | Dec | source scan (no codegen, no `.parse` on API data) | 2 |

### Security and identity (cross-cutting)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0030 | Tenants are isolated by site (query filters for financial data, site-scoped repositories for users and sites, named cross-tenant reads); a foreign target reads as missing (404) | Acc | PRD-026; server DEVELOPER:4639–4741 | 1 |
| 0031 | Authorisation fails closed: authentication by default, an explicit anonymous allow-list, pinned by test | Acc | PRD-026:437 | 1 |
| 0032 | `resource:action` permissions, resolved from the database on every request | Acc | AUTHENTICATION:1902–2017; AddRolesAndPermissions migration | 1 |
| 0033 | Platform administration is granted by deployment configuration, not a role | Acc | AUTHENTICATION:2021–2155; PRD-026:518; PRD-027:75 | 1 |
| 0034 | A site's last enabled Admin and the platform's last live platform administrator are protected | Acc | PRD-026:439–441 as amended by PRD-022:43–57 | 1 |
| 0035 | Sign-up requires email verification and platform-admin approval; rejected usernames stay locked | Acc | AUTHENTICATION:24–256 | 2 |
| 0036 | Credentials: PBKDF2 hashing via the Identity hasher (without Identity), single-use one-time codes, enumeration-safe reset with temporary password | Acc + Dec (plaintext codes) | AUTHENTICATION:568–944; UserPasswordHasher.cs; OneTimePasswordEntity.cs; PRD-032 may amend | 1 |
| 0037 | Short-lived access tokens with minimal claims plus server-side AuthSessions with a rotating HttpOnly refresh cookie | Acc | PRD-001; PRD-002; AUTHENTICATION:1258–1313, 1844–1898 | 1 |
| 0038 | Revocation per session (`session_id` checked on every request) and globally (`TokenVersion`) | Acc | PRD-002:55; commit `31c96313` (2026-10-17) | 1 |
| 0039 | One server-side chained rate-limit policy; accounts are never locked | Acc + Ver (proxy IP, see §11) | server DEVELOPER:3502–3631; PRD-014:27–37; user-guide Security | 1 |
| 0040 | Credentialed CORS only from configured origins, exposing only the headers the client needs | Acc | server DEVELOPER:3413–3501 | 2 |

### Backend

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0041 | Layered server (API → App → Data, plus Shared) with feature slices mirrored across layers; auth lives in the host | Acc | ARCHITECTURE:95–118; server.instructions:10–15 | 1 |
| 0042 | Endpoints are Minimal API slices: static handler, typed results, separate request/response models, hand-written mapping (no AutoMapper) | Acc | server DEVELOPER:439–488, 2919–3085 | 1 |
| 0043 | Build on the AllOverIt library family | Acc | Directory.Packages.props; allsoverit-patterns.instructions | 1 |
| 0044 | Services are registered by marker-interface scanning | Dec (conflicts with server.instructions:61–65) | server DEVELOPER:5026–5223 | 2 |
| 0045 | Business failures are `EnrichedResult` values, not exceptions | Acc | server DEVELOPER:2227–2304; PRD-030:138 | 1 |
| 0046 | Request shape is validated in the handler; data-dependent rules are entity-check chains | Acc | server DEVELOPER:339–367, 2040–2225, 3267–3411 | 1 |
| 0047 | Split read/persist repositories, no-tracking by default, explicit transactions | Acc | server DEVELOPER:2368–2802 | 1 |
| 0048 | Persistence conventions: `Entity` suffix, enriched enums stored by name, `citext` with check-plus-index uniqueness | Acc | server DEVELOPER:4079–4635 | 2 |
| 0049 | Cascade delete is restricted; deletions are ordered hard-delete sweeps; no soft delete | Acc | server DEVELOPER:4531–4550; PRD-022 | 1 |
| 0050 | A separate runner applies migrations before the API starts; destructive changes ship in two append-only steps; humans generate migrations | Acc | PRD-023:313–324; PRD-013; entrypoint.sh | 1 |
| 0051 | PostgreSQL version target | Dec (13.6 pinned, EOL; Azure runs 17.6) | DbContextOptionsBuilderExtensions.cs:11; Postgres Dockerfile; Azure .mmd | 1 |
| 0052 | Configuration binds to validated options sections that fail at startup | Acc | server DEVELOPER:5022; server.instructions:27 | 2 |
| 0053 | Liveness is separate from readiness; background workers wait for readiness | Acc | PRD-013; PRD-030:67 | 2 |
| 0054 | Background work runs in the API process (single-replica assumption) | Acc + Dec | Features/Workers; PRD-019 proposes an outbox | 2 |
| 0055 | Email is queued in process and rendered with Razor components | Acc (PRD-019 proposes superseding) | Pot.EmailSender; Pot.RazorComponents | 2 |
| 0056 | Log request metadata only, never submitted values; structured exception logging; JSON console in production | Acc | PRD-030 A1–A6; allsoverit-patterns:64–68 | 1 |
| 0057 | Middleware order is a contract; security headers on every response | Acc | Program.cs:50–125; server DEVELOPER:3633–3822 | 2 |
| 0058 | Server tests: per-layer unit projects plus one hosted integration project on Testcontainers; architecture rules pinned by fixture tests | Acc | ARCHITECTURE:191–211; dotnet-tests.instructions | 1 |
| 0059 | Server tests stay on VSTest (`xunit.v3.mtp-off`) until MTP mode works | Acc | PRD-020; dotnet-tests:27 | 3 |
| 0060 | Build quality gates: nullable warnings are errors, `.editorconfig` is the style source, central package versions with major-version ranges | Acc + Prop (PRD-029) | PRD-029; Directory.Build.props / Directory.Packages.props | 2 |

### Frontend

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0061 | Feature modules over shared `api/`, `concerns/` and `lib/` layers, with one-way dependencies | Acc + Ver (existing feature→feature imports must be sanctioned or scheduled) | client DEVELOPER:106–261; client.instructions:12–18 | 1 |
| 0062 | shadcn/ui primitives are vendored and never hand-edited | Acc | PRD-015:33; client.instructions:16 | 1 |
| 0063 | Server state lives in TanStack Query over one axios instance; invalidation follows a dependency graph | Acc | client DEVELOPER:2339–2705; cacheInvalidation.ts | 1 |
| 0064 | API hooks return `Result` values; one interceptor normalises every failure and never fails on a body | Acc | client DEVELOPER:2612–2723; PRD-030 E1–E8 | 1 |
| 0065 | One blocking ErrorSheet, transient toasts and a root error boundary | Acc | client DEVELOPER:1870–2322; App.tsx | 2 |
| 0066 | Client state split by kind (Zustand persisted user, Query, Context, React Hook Form) | Acc | ARCHITECTURE:269–274; client DEVELOPER:2323 | 2 |
| 0067 | Browser storage is scoped by environment and user, with a persistence policy per setting | Acc | PRD-001 (amended by PRD-011, PRD-025) | 1 |
| 0068 | The URL is the only render-time source of truth for list filters | Acc | PRD-011 | 1 |
| 0069 | Lazy declarative routes behind `ProtectedRoute`; create/edit forms are route-driven non-modal sheets | Acc | AppRoutes.tsx; client DEVELOPER:1354–1595, 2545–2611 | 2 |
| 0070 | Forms use React Hook Form with Zod; API types are derived with `z.infer` | Acc | client DEVELOPER; feature schemas | 2 |
| 0071 | Client token handling: access token in memory only, proactive and reactive refresh, one logout path | Acc + Ver (the docs disagree on the refresh model) | AUTHENTICATION:948–1488; authInterceptor.ts; logoutManager.ts | 1 |
| 0072 | Honour `Retry-After` and never auto-retry a 429 (re-homes legacy ADR 014) | Prop | ADR-014; PRD-014 | 2 |
| 0073 | Client configuration is baked in at build time | Acc | Docker DEVELOPER:366–386, 490–502 | 2 |
| 0074 | PWA precaches only the shell, never `/api`, and enforces updates | Acc | client DEVELOPER:2939–3125; vite.config.ts | 1 |
| 0075 | Login alone gets a longer timeout with progress feedback | Acc | PRD-006 | 3 |
| 0076 | Strict TypeScript and lint policy; no React Compiler | Acc | eslint.config.js; tsconfig.app.json; PRD-015 DQ-7 | 2 |
| 0077 | Client dependencies upgrade in gated micro-batches; TypeScript 7 waits for typescript-eslint | Acc | PRD-015; PRD-016; PRD-017 | 2 |
| 0078 | Client unit tests use Vitest with module mocks; MSW is deferred behind stated triggers | Acc | client DEVELOPER:2837–2915 | 2 |

### End-to-end testing (cross-cutting; replaces legacy ADR 012)

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0079 | E2E runs one shared stack with Testcontainers Postgres and a single seeded database | Acc | ADR-012 §3–4, §8–9, App. A | 1 |
| 0080 | Every E2E test is classified as parallel-safe or fixture-managed | Acc | ADR-012 §7 | 1 |
| 0081 | Every E2E test logs in fresh; storage state is never shared | Acc | ADR-012 §6 | 2 |
| 0082 | E2E execution policy: dev matrix by default, prodlike gate, fixed timeouts, config-owned reporters, four-browser matrix | Acc + Dec (prodlike: per-PRD gate or on demand?) | ADR-012 §10, §13; e2e README:130–133; e2e.instructions:13 | 2 |
| 0083 | Scenario-driven tenant testing: test-only mail sink, product-route provisioning, canary tenant (amends 0079) | Prop | PRD-031 | 2 |

### Infrastructure

| # | Decision | Status | Main sources | Tier |
| --- | --- | --- | --- | --- |
| 0084 | Production runs on Azure Container Apps from GHCR images; nginx serves the SPA; the API is on its own subdomain | Ver (docs disagree on whether ingress routes `/api`) | Azure .mmd; Docker DEVELOPER:665–714; AUTHENTICATION:1612–1662 | 1 |
| 0085 | Multi-stage, non-root Alpine images containing runtime projects only | Acc | PRD-007:61–114; Dockerfiles | 2 |
| 0086 | Images build with the cache by default | Sup by 0087 | PRD-007:56–76 | 3 |
| 0087 | Images always build with `--no-cache` | Acc | PRD-007:246–264 | 2 |
| 0088 | Local environments: prodlike compose stack, separate dev and E2E databases on bind mounts, fixed ports, one stack at a time | Acc | Data/README; Docker DEVELOPER | 2 |
| 0089 | Secrets stay out of the repo: a tracked template, an ignored env file, the platform secret store in production | Acc + Ver | GETTING-STARTED:103–126; Docker DEVELOPER:415–454 | 2 |
| 0090 | Build and deploy by hand from VS Code tasks; no CI | Dec | .vscode/tasks.json; empty .github/workflows | 2 |

**Size check.** The ADR expert's view was that above about 50 records an ADR set starts to become a decision-shaped copy of the developer guide. Tier 1 is 50. If you want a leaner set, drop or fold Tier 3 and the smaller Tier 2 rows: 0005, 0026, 0027, 0040, 0052, 0057, 0065, 0066, 0073 and 0081 all fold naturally into a neighbour.

> **Your call on the backlog: Fold as recommended - related neighbours can live together**

**Outcome:** the ten rows are folded into their neighbours when the ADRs are written, and their numbers are not used. The backlog is renumbered into a clean sequence once D13 is settled, because D13 may turn some convention rows into rules.

---

## 9. Things that are not ADRs

These stay as rules, examples or reference, with no ADR of their own:

- **C# style:** sealed by default, no primary constructors, one class per file, member ordering, `CancellationToken` last, no `ConfigureAwait(false)`, XML docs. These become rules in `Docs/standards/server/` (D13 option A), with `.editorconfig` taking precedence for anything it checks. ADR-0060 covers *how* style is enforced.
- **TypeScript style:** function declarations, exports at the bottom, barrel files, braces. These become rules in `Docs/standards/client/`.
- **Component catalogues:** tables, cards, badges, pickers, theme, chart sizing. These go in the client reference.
- **Single-place reference facts:** ports, the env-var list, the permission list, the token claim list, the seeded E2E identities.
- **Small API conventions:** 201 for create and 200 for delete, action-style POST for bulk operations, the login `status` discriminator. These go in the API reference, or are folded into ADR-0023 or 0035.
- **Build details:** manual vendor chunks, the logger singleton, the accessibility-first locator order in E2E.

---

## 10. Content to correct during the migration

Each item is fixed when its file is reworked, not in a separate pass.

**Fix early: wrong in a security-relevant way, or misleading enough to cause bad code**

1. **Server DEVELOPER 3983–4045:** the `DatabaseExceptionHandler` sample echoes `PostgresException.MessageText` and maps everything to 500. The real handler returns 409 or 500 with fixed messages and never echoes database text.
2. **AUTHENTICATION 923 and 2163 say BCrypt.** The code uses the Identity `PasswordHasher` (PBKDF2).
3. **AUTHENTICATION 461, 469, 836, 1335 and 1417–1460 describe the removed `User.RefreshToken` design.** Lines 1460 and 2166 say there is "no server-side session store", which contradicts AuthSession.
4. **Access-token storage contradicts itself.** AUTHENTICATION 1181 and 1246 say sessionStorage, while 1269 and 1300 say memory only. The refresh model at 1826–1832 contradicts 952–971.
5. **The "Site Owner" role doesn't exist.** The user guide (README 77–79 and every page) and `_DOCUMENTATION_PLAN.md` use it; the real roles are Admin and Viewer. AUTHENTICATION 1919 lists a non-existent `import-export:manage` permission.
6. **ARCHITECTURE 284–291 says production is Docker Compose with Docker PostgreSQL.** It is Azure Container Apps with a separate PostgreSQL 17.6.

**Fix when the file is reworked**

7. **ARCHITECTURE.md:**
   - The tree (38–79) shows `store/` and omits `Pot.Shared.Tests`, `Pot.RazorComponents` and `contracts/`.
   - The stack list omits AllOverIt, TanStack Table, the PWA, Scalar and Tailwind v4.
   - Line 272 says Zustand holds authentication; the access token is in React context.
8. **Server DEVELOPER:**
   - JWT claims (239, 3250–3257) omit `session_id`.
   - The enriched-enum sample (4323) omits `EndOfMonth`.
   - Line 206 lists 204, but deletes return 200.
   - Line 1874 (`WhenNotNull` on every dependency) contradicts allsoverit-patterns:18.
   - The table of contents is wrong.
9. **IMPLEMENTATION.md:**
   - Encoding corruption at about 20 lines.
   - `AccruedIsDirty` at 442 and 603 no longer exists.
   - The projection mechanism text (374–381, 732–740, 767–777, 802, 804–813) predates the fold or cursor design.
   - `EndOfMonth` is never mentioned.
10. **Client DEVELOPER:**
    - The source tree (46–101) is stale.
    - It claims ESLint "strict" (262–323), but the config is `recommended` + `stylistic`.
    - Filter persistence is described as localStorage (1854–1866); the code uses the URL plus sessionStorage.
    - Auth and logout paths are wrong (2391–2427).
    - The cache invalidation path is wrong (2678).
    - It lists a `ValidationError (422)` that doesn't exist (2707–2726).
    - It imports from `react-router-dom` (2556).
    - Chart heights are stale (1689–1759).
    - The toast import points at a non-existent hook (765).
11. **E2E docs:**
    - `e2e/README.md:10`, `e2e/AUTHORING.md:5` and `ADR-012:24` link to a non-existent `playwright-e2e.instructions.md`.
    - The fixture lists in AUTHORING and e2e.instructions are stale (three listed, five exist).
    - `e2e/README.md:352` refers to a "see 8.7" that matches nothing.
    - Playwright and Vite versions are stale.
    - e2e.instructions:13 calls prodlike a per-PRD completion gate, while the README calls it on-demand.
    - e2e.instructions:63 says react-day-picker v9; the project uses v10.
12. **Setup docs:**
    - Node minimum: 22.22.2 vs `>=22.22.0` vs "Node 24 is the minimum".
    - `.env.development`: "create from scratch" vs "copy the tracked template" (the template exists).
    - `Docs/Azure/Azure-Deployment-Notes.md` is linked three times but doesn't exist.
    - The GETTING-STARTED anchor at 228 is broken.
    - DOCKER-SETUP's "curl" output is actually PowerShell output.
    - `Data/README.md` 96 and 182 give the prodlike port as 5432; it is 5444.
    - LOCAL-SETUP Option B collides with the prodlike container name.
13. **FIRST-TIME-SETUP:**
    - Status names are inconsistent: `PendingApproval` vs `Approval`.
    - It says only platform admins see the Users menu; the real gate is `user:manage`.
14. **Other docs:**
    - FEATURES 145 claims automatic logout on inactivity, which doesn't exist.
    - USER-GUIDE/Security 7 says a password change ends "every other session"; it ends every session, including the current one.
15. **Compose file comment:** it refers to `/swagger`, but the API UI is Scalar and is Development-only.

---

## 11. Risks found in the code (for information)

These came out of the code scan. They are **not** documentation work and I have not acted on them. Several are why rows in section 8 are marked `Dec` or `Ver`. The first four I verified myself.

**Round 2:** every item below, including the smaller ones, now has a brief in `DECISIONS-TO-DISCUSS.md` (D10). Decide them there; this table is kept for reference only. The briefs were checked against the code and correct several rows below: PostgreSQL has three versions in use, not two; EF InMemory is used mainly in `Pot.Data.Tests`; and the forwarded-headers problem is wider than described here. Where a brief and this table disagree, the brief is right.

| Risk | Detail |
| --- | --- |
| **Anonymous rate limiting may partition on the proxy's IP** | There is no `UseForwardedHeaders` anywhere in the server. Behind nginx or Azure ingress, `RemoteIpAddress` is the proxy, so every anonymous caller would share one 15-requests-per-10-seconds bucket. |
| **The Etag is compared in application code but is not a database concurrency token** | Two writers that both pass the check both succeed. Saves within the same millisecond share an Etag. The client types `etag` as `z.bigint()`, but JSON gives a JavaScript number. |
| **PostgreSQL 13.6 is pinned** (`SetPostgresVersion`, `FROM postgres:13`) | 13 reached end of life in November 2025. The Azure diagram says production is 17.6. |
| **Money is `double`** end to end | — |
| **One-time codes (`OtpCode`, `RefCode`) are stored in plaintext** | They are protected only by expiry, attempt limits and cleanup. |
| **Email is queued in an in-process channel** | Queued mail is lost on restart. Workers assume a single replica, so more than one replica would send duplicate reminders. PRD-019 addresses this. |
| **The client does not consume the committed OpenAPI document** | There is no codegen and no runtime validation of API data. |
| **Permission policies are string literals at endpoints** | A typo compiles, then returns 403 for everyone. |
| Smaller items | `Cronos` is referenced but unused. `components/ui/sonner.tsx` uses `next-themes` while the app has its own ThemeProvider. `tailwindcss-animate` is a v3 plugin in a v4 setup. App unit tests run on EF InMemory, which ignores `citext` and constraints. The ERD export path is hard-coded to `Docs/`. |

---

## 12. Changes to skills, templates and agent instructions

| File | Change |
| --- | --- |
| `.github/skills/prd-discovery/references/prd.template.md` | **Moves to `Docs/prd/template.md`** (D8); the skill links to the new path. Add a commented `**Amended:**` header line, used only once amended. Add an `ADR` column to "Decisions Carried Into This Document". Add an `## Effect on Earlier Documents` section (Document, What changes, Amendment). Add a closing `## Amendments` section containing "None." |
| `.github/skills/prd-discovery/SKILL.md` | **Stage 5:** classify each carried decision as product or architectural, apply the significance test (5.3), and draft a `proposed` ADR for each architectural decision. **Before deleting the discovery document**, harvest the rejected alternatives into those ADRs; today that rationale is lost when the scratch file is deleted. Fill in "Effect on Earlier Documents". Update the paths if D5 is approved. |
| `.github/skills/prd-delivery/SKILL.md` | **Step 4 (verify):** add a check listing decisions taken during delivery that are not covered by an ADR, and raise or justify each. **Step 5 (close):** flip the PRD's ADRs to `accepted`; write an `AM-n` entry into every document named in "Effect on Earlier Documents", plus any other contradicted Complete PRD; keep the index row to one line. Replace "per `Docs/Future` conventions" with a link to ADR-0003. |
| `.github/skills/prd-delivery/references/stage-plan.template.md` | Path update (D5). |
| `.github/skills/adr/SKILL.md` (new) | Create an ADR from `Docs/adr/template.md` following `Docs/adr/README.md`, take the next number from the folder, fill in the front matter (with evidence for retroactive records), update the related instruction-file citations, and run `docs-check`. Never stages or commits, like the other skills. |
| `Docs/adr/README.md` (new) | A hand-written "How this works" section (D8) above the generated index. |
| `.github/scripts/docs-check/` (new) | `docs-check.ps1`, `README.md`, Pester tests and fixtures (D11). Add a row for it to `.github/scripts/README.md`. A VS Code task runs it. |
| `Docs/standards/` (new, if D13 option A) | Rule documents, each starting with a quick-reference list of its rules and their enforcement tags, as ClearWeigh does. |
| `Docs/Future/README.md` (becomes `Docs/prd/README.md`) | Add a hand-written "How this works" section (D8). Remove the "next free ADR number" text. Define the amendment convention by linking ADR-0003. Add the `Amended` column. Trim the summaries that carry supersession history (003, 009, 015, 023, 026). |
| `.github/copilot-instructions.md` | **Line 14:** keep the ban on PRD and ADR IDs in code, but change its reason, which currently calls these documents "temporary". New wording: "PRDs and task lists are temporary; ADRs are permanent but are cited from docs and instruction files only — code states the rule itself." **New rule:** "Before an architectural change, read `Docs/adr/README.md`; do not contradict an accepted ADR without asking; a change that makes a new architectural decision adds a proposed ADR in the same change." **References section:** point at the new paths. |
| `.github/instructions/*.instructions.md` | Append the ADR number to each rule cluster, for example `… a foreign id is a 404 (ADR-0030).` Rules stay imperative; the rationale is not copied in. |
| `AGENTS.md` (D12, approved) | A short root file that points at `.github/copilot-instructions.md`, the instruction files, `Docs/README.md`, `Docs/adr/README.md` and the rule documents. It does not restate their rules. Created in Phase 1. |

---

## 13. Migration phases

Each phase ends with a stop for your review and a commit message offered. I never stage or commit.

| Phase | Work | Output |
| --- | --- | --- |
| **0. Agree** | You comment on this file and decide D13; I revise it. In parallel, you work through `DECISIONS-TO-DISCUSS.md` at your own pace. Nothing in the rest of the plan waits for it. | An agreed plan. |
| **0b. Re-baseline** | The executing agent compares the baseline `68ee3480` with the branch it will change, following the steps in [Before executing this plan](#before-executing-this-plan-re-baseline-first). It does this immediately before Phase 1, after the planned documentation updates have landed. | A dated re-baseline addendum, and your OK to start Phase 1. |
| **1. Foundations** | Create `Docs/adr/` with `template.md`, the "How this works" README, ADR-0001 to 0004 and the generated index. Move the PRD template to `Docs/prd/template.md` (still under `Docs/Future/` until Phase 5) and write its "How this works" section. Write `Docs/README.md` (docs map plus the "where does X go" table). Build `docs-check.ps1` with its Pester tests and VS Code task. Make the skill and copilot-instructions changes from section 12. Create the `adr` skill and `AGENTS.md`. If D13 is option A, create `Docs/standards/` with an empty rule-document template and the standards ADRs. | Conventions in place before any content moves. |
| **2. Re-home the legacy ADRs** | Write ADR-0079 to 0082 from ADR-012 and ADR-0072 from ADR-014. Move ADR-012's reference content into `e2e/README.md`. Leave tombstones at the old paths. Fix the broken instruction-file link. | ADR-012 and ADR-014 retired. |
| **3. Write the ADRs in themed batches** | Batches of 5–10 ADRs per commit, in this order: domain, then the API contract, security, backend, frontend, infrastructure. Each batch also adds the ADR citations to the instruction files. The `Ver` rows are checked against the code first. Each `Dec` row is written once its item in `DECISIONS-TO-DISCUSS.md` is decided. | The ADR series. |
| **4. PRD amendments** | Retrofit the 29 existing notes (D3) and add the missing amendments (D4), one PRD per change. The new ADRs give the 422→400 and session-binding amendments something to cite. Clean up the index summaries. | Every overtaken PRD says so in one predictable place. |
| **5. Restructure** | Move and split files into the section 6 layout: slim ARCHITECTURE.md, split AUTHENTICATION.md, split the developer guides per Appendix E (rules into `Docs/standards/`, examples beside the code), build the glossary and the reference pages, apply the section 10 corrections as each file is touched, and rename the folders (D2, D5). The ERD path in `App.cs` changes only with your approval. `docs-check` must pass before each commit. | The target structure. |
| **6. Steady state** | The governance rules in section 14 apply from here on. | — |

**Why restructuring comes last:** Phases 3 and 4 only *add* files and sections. Phase 5 rewrites each existing file once, replacing duplicated rationale with links to ADRs that by then exist. Doing the moves first would mean editing most files twice.

---

## 14. Governance: keeping it from sprawling again

1. **A decision becomes an ADR in the same change.** Agents add it as `proposed`; the skills enforce this for PRD work.
2. **A change that contradicts a Complete PRD adds an `AM-n` entry to it in the same change.**
3. **Every document type has exactly one home**, set by the `Docs/README.md` table. Prose never restates code, manifests or the OpenAPI document.
4. **Size budgets:**
   - ADRs: about 150 lines or fewer.
   - Developer-guide topic files: about 800 lines or fewer.
   - `ARCHITECTURE.md`: about 250 lines or fewer.
   - Instruction files: "concise".
5. **`docs-check` passes before commit.** It checks:
   - the front matter;
   - that the generated index is current;
   - that relative links resolve;
   - that instruction-file citations point to accepted ADRs;
   - that no `PRD-`/`ADR-` IDs appear in `Source/**`.
6. **Review checklist** for any change:
   - Does it make a decision, and is there an ADR?
   - Does it contradict an ADR or a Complete PRD?
   - Were generated files (index, OpenAPI) regenerated?
   - Does `docs-check` pass?

---

## Appendix A — ADR template

```markdown
---
id: ADR-0030
title: Isolate tenants by site; a foreign target reads as missing
status: accepted              # proposed | accepted | rejected | deprecated | superseded
date: 2026-10-14              # when the decision was made (backfill: best evidence)
recorded: 2026-11-04          # when this record was written
provenance: retroactive       # contemporaneous | retroactive
evidence:                     # required when retroactive
  - ../prd/PRD-026-cross-tenant-authorization-hardening.md#4-design
  - ../../Source/Server/Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs
area: cross-cutting           # frontend | backend | cross-cutting | infrastructure | process
tags: [security, multi-tenancy, authorization]
supersedes: []
superseded-by: []
amends: []
amended-by: []
related-prds: [PRD-026, PRD-022, PRD-027, PRD-031]
related-adrs: [ADR-0031, ADR-0034]
---

# ADR-0030: Isolate tenants by site; a foreign target reads as missing

**Summary:** In the context of a multi-tenant API where users and sites cannot carry a global query
filter, facing cross-tenant reads and writes through caller-supplied ids, we decided to remove raw
user/site queryables and resolve every target through site-scoped repository methods that report a
foreign id exactly as a missing one (404), to make isolation structural rather than per-route,
accepting a narrower repository API and a closed list of named cross-tenant reads.

## Context and Problem Statement
<3–10 lines: the forces, not the history of the discussion. Link the PRD section for detail.>

## Decision Drivers            <!-- optional -->
- <driver>

## Considered Options
1. Per-route site predicates (status quo before PRD-026)
2. Global query filters on `User` and `Site`
3. Site-scoped repository surface with no raw queryables

## Decision Outcome
Chosen option: 3, because <≤15 lines>.

### Consequences
- Good, because <…>
- Bad, because <…>

### Confirmation
- Test: `Pot.AspNetCore.Integration.Tests/Security/CrossTenantAuthorizationFixture.cs`
- Instruction: `.github/instructions/server.instructions.md` (Architecture)
- Review: any new id-taking endpoint resolves its target through a site-scoped method.

## Pros and Cons of the Options   <!-- optional -->

## More Information               <!-- optional: links and follow-ups; never implementation status -->
```

---

## Appendix B — PRD amendment format

**In the PRD that was overtaken.** The header gains one line; the body is untouched.

````markdown
**Status:** Complete
**Priority:** High
**Last Updated:** 2026-10-14
**Amended:** 2026-10-26 — 2 amendments, see [Amendments](#amendments)
**Feature ID:** 026

…(the PRD as approved — never edited)…

## Amendments

Append-only, oldest first. The text above is this document as approved; read it through these
entries. Each entry names what it affects and the document or change that caused it.

### AM-1 — Refusals answer 400, not 422 (2026-10-17)

- **Type:** Superseded
- **Affects:** every `422` in this document (§4.3 refusals; AC5)
- **Source:** [ADR-0025](../adr/0025-validation-and-refusals-answer-400.md) (commit `bec69a48`)

This document records `422 Unprocessable Entity` for the refusals it introduced. That status code
has since been replaced: every invalid or refused request now returns `400 Bad Request` with the same
ProblemDetails body … *(moved verbatim from the original top-of-file note)*

### AM-2 — A refusal's message is read from the `errors` map (2026-10-26)

- **Type:** Superseded
- **Affects:** how a refusal's message is read (the `400` responses of AM-1)
- **Source:** [PRD-030](PRD-030-problem-details-consolidation.md), [ADR-0023](../adr/0023-problem-details-contract.md)

A `400` now carries the validation problem's `errors` map … *(moved verbatim)*

### AM-3 — A platform administrator may be removed while another remains (2026-10-27)

- **Type:** Superseded
- **Affects:** §4.4, §4.8, D2, AC5
- **Source:** [PRD-022](PRD-022-user-and-site-deletion.md) R7–R9; [ADR-0034](../adr/0034-protected-identities.md)

*(new entry, D4)*
````

An in-body pointer is used only where a reader would otherwise act on a now-wrong paragraph:

```markdown
> Amended — see [AM-1](#am-1--refusals-answer-400-not-422-2026-10-17).
```

**In the PRD that causes the change.** This is a new template section, filled in during discovery:

```markdown
## Effect on Earlier Documents

| Document | What changes                                           | Amendment |
| -------- | ------------------------------------------------------ | --------- |
| PRD-026  | A platform administrator is no longer always protected | AM-3      |
| PRD-027  | Notification and protection rules it reuses change     | (rewrite) |
```

---

## Appendix C — Existing amendment notes to retrofit

These 29 sites were found by the PRD audit. Under D3 option A, each moves verbatim into an `AM-n` entry in its own document.

| # | Location | Current format | Cause | Date |
| --- | --- | --- | --- | --- |
| 1 | PRD-001:357–359 | unlabelled paragraph in a list | session binding (commit `31c96313`; belongs to PRD-002) | 17 Oct 2026 |
| 2 | PRD-003:34–38 | status bullets + "Supersession note" | PRD-023 | 2026-09-27 |
| 3 | PRD-003:70 | section blockquote "Snapshot note" | PRD-023 | 2026-09-27 |
| 4 | PRD-007:246–264 | appended H2 "Supplementary Implementation Note" | own incident (cache-first reversed) | 2026-05-03 |
| 5 | PRD-009:5–10 | header bullets + "Supersession note" | PRD-023 | 2026-09-27 |
| 6–7 | PRD-012:104, :195 | blockquotes under R2, R10 | PRD-023 | 2026-09-27 |
| 8 | ADR-012:325 | blockquote in a numbered step (also stands for :112, :178) | PRD-023 | 2026-09-27 |
| 9 | ADR-012:361–363 | undated parenthetical; **wrongly credits PRD-030** | 422→400 (`bec69a48`) | — |
| 10 | PRD-014:7 | Scope header rewritten | CORS exposes `Retry-After` | 2026-10-19 |
| 11 | PRD-014:35 | undated parenthetical | PRD-030 | — |
| 12 | PRD-014:36, ADR-014:37–39 | trailing "Added …" sentence | CORS exposure | 2026-10-19 |
| 13 | PRD-015:14–99 | dated H3 decision log, "RESOLVED" markers | own living log | 2026-09-03 to 09-21 |
| 14 | PRD-018:32 | blockquote in a list | PRD-023 | 2026-09-27 |
| 15 | PRD-019:43 | blockquote in a bullet | PRD-023 | 2026-09-27 |
| 16–18 | PRD-023:144, :146, :324, OD table cells | bold inline "Amended …", table-cell markers | own post-implementation changes | 2026-09-27 |
| 19 | PRD-023:220, :385 | italic parenthetical | PRD-025 | 2026-10-03 |
| 20 | PRD-024:40 | blockquote after a list | PRD-023 | 2026-09-27 |
| 21 | PRD-025:7 | italic parenthetical inside Scope | PRD-028 (no back-link) | 2026-10-04 |
| 22 | PRD-025 (several rows) | "updated / dropped on …" in tables | own rework | 2026-10-03 |
| 23 | PRD-026:3–7 | top blockquote "Superseded contract note" | 422→400 (no PRD) | 2026-10-17 |
| 24 | PRD-026:9–13 | top blockquote "Problem Details note" | PRD-030 | 2026-10-26 |
| 25 | PRD-027:3–17 | top blockquote "Needs rewriting" | several | 2026-10-17+ |
| 26 | PRD-028:7 | italic parenthetical inside Scope | own post-completion rename | 2026-10-04 |
| 27 | PRD-022:172 | undated inline sentence | own test-plan rewrite | — |
| 28–29 | `Docs/Future/README.md` rows 003, 009, 015, 023 | clauses in the Summary column | PRD-023 and own changes | various |

**Two exceptions:**

- **Sites 13 and 16–22** are a PRD amending *itself* during or straight after delivery. They are part of that PRD's own record, so I'd leave them in place unless you want them moved too.
- **Site 25** is PRD-027, which is not delivered; it gets folded into its rewrite instead.

> **Your call on the exceptions: As per your recommendation**

---

## Appendix D — PRDs affected by later decisions that carry no note

Under D4, each of these gets a new `AM-n` entry.

| Document | What is now wrong | Cause |
| --- | --- | --- |
| PRD-026 §4.4:441, §4.8:493/504, D2:603, AC5:618 | A configured platform administrator can never be disabled | PRD-022 R7–R9 |
| PRD-026 §4.6:465 | Token claims "…and nothing else" | `session_id` (commit `31c96313`) |
| PRD-026:625–626 | "Blocked pending this document" | PRD-022 and PRD-026 delivered |
| PRD-002 | No note on session binding (it was put in PRD-001); open decisions unanswered although Complete; title still says "Placeholder" | commit `31c96313` |
| PRD-001:94 | Legacy keys are purged | PRD-025 R11 removed the purge |
| PRD-001:78–80 | Storage read on render (S004–S006) | PRD-011 (URL is authoritative) |
| PRD-009:155–157 | Package "current version is v3" | PRD-021 (v4) |
| PRD-021:91–97 | Any column change bumps the version | PRD-023 OD-02 changed v4 in place |
| PRD-012 NG5, D5, C2–C3, R3 / ADR-012 §1.3, §2, §14.2 | No email E2E; committed seeds only | PRD-031 mail sink and route provisioning |
| ADR-012 §5:185–203 | Three seeded users | PRD-022 and PRD-031 (six identities) |
| PRD-012 C7:245 / ADR-012:107, :133 | Vite 6, Playwright 1.59 | PRD-015 |
| ADR-014:34–36 | Error code `TooManyRequests` in the body | PRD-030 removed `errorCode` |
| PRD-014 OQ-6:76 | Can the server omit `Retry-After`? | Answered by PRD-030 B8 |
| PRD-013 | Probes not described as short-circuited | PRD-030:67, :143 |
| PRD-018 Q5:113, :149 | Points at PRD-009 dirty-flag behaviour | PRD-023 |
| PRD-019:30, R9, OD-04 / :41 | "Seven" email writers; old cross-tenant method name | PRD-022 notifications; PRD-026:457 |
| PRD-025 R5, R6, R8, payload table, Tests | `global` / "Total (All Accounts)" | PRD-028 |
| PRD-028:55 | "Implementation has not started" (Status is Complete) | Corrected |
| PRD-007:3, :54–76 | Header "Planned"; cache-first decisions | Corrected / own incident |
| `Docs/Future/README.md` rows 007, 026, 027 | Cache-first; "refuse (422)"; "Blocked" with no rewrite flag | as above |

---

## Appendix E — Developer guide classification

This appendix sorts every section of the three developer guides into the layers agreed in D6:

- **Decision:** goes to an ADR.
- **Rule:** goes to a rule document (D13).
- **How-to:** worked examples, which stay beside the code.

It also flags sections that are **Tool-enforced**, **Duplicate** or **Stale**. A read-only sub-agent produced it at commit `ffc2e2b7`, and every code check it made is named in the Notes column.

- **Guides covered:**
  - `Source/Server/DEVELOPER.md` (5,308 lines)
  - `Source/Client/pot-react/DEVELOPER.md` (3,228 lines)
  - `Source/Docker/DEVELOPER.md` (840 lines)
- **Method:**
  - I read every section's prose in full.
  - I sampled the code blocks.
  - I checked claims against `Source/` where that was cheap. Every check is named in the Notes column.
- **Reading the Mix column:** it estimates how each section's lines split between Decision, Rule and How-to, in that order. Code blocks count as How-to, so example-heavy sections score high on How-to even when their prose is mostly rule.
- **Folded backlog rows:** these are cited through the row I propose should absorb each one. Please confirm the hosts in the call at the end of this appendix:

  | Folded row | Assumed host |
  | --- | --- |
  | 0005 | 0004 |
  | 0026 | 0023 |
  | 0027 | 0023 |
  | 0040 | 0037 |
  | 0052 | 0060 |
  | 0057 | 0056 |
  | 0065 | 0064 |
  | 0066 | 0063 |
  | 0073 | 0085 |
  | 0081 | 0080 |

- **Naming conventions:**
  - Rule-document topics are written as `server/<topic>` and `client/<topic>`.
  - How-to files are written as `docs/<name>.md`, relative to the component folder (`Source/Server/` or `Source/Client/pot-react/`).


### E.1 Server guide: `Source/Server/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + Table of Contents | 1–60 | – | Delete | The new ≤150-line index | The TOC is wrong (plan §10.8). Regenerate it as part of the split. |
| Architecture Overview / Project Structure | 61–147 | 5/15/80 | How-to | 0041; rule `server/project-structure`; `docs/project-structure.md` | The tree is nearly current but misses `Pot.App/Mappings`, `Pot.AspNetCore/Concerns/OpenApi` and `Pot.Shared/Models` (I checked with `ls`). The test-project list duplicates dotnet-tests.instructions:8. Replace it with one paragraph per layer plus a link. |
| Key Architectural Principles | 148–198 | 70/30/0 | Decision (+Duplicate) | One bullet group per ADR: 0032/0031 (1), 0041 (2), 0046/0023/0025 (3), 0045 (4), 0042 (5), 0030 (6), 0053 (7), 0047 (8); rate numbers → 0039 | This duplicates Security (232–248) and ARCHITECTURE 95–118. Line 152 says "role-based authorization", but permissions are resolved from the database (0032). The plan already says to turn this section into ADR links. |
| Design Conventions | 199–231 | 20/70/10 | Rule | 0021, 0022, 0042, 0045, 0048; rules `server/project-structure` (naming), `server/endpoints`, `server/error-handling` | Good rule material (naming patterns, RowId only). Line 206 lists 204, but deletes return 200 (plan §10.8). |
| Security | 232–248 | 65/30/5 | Decision | 0030, 0031, 0032, 0033, 0034, 0036, 0037, 0038, 0039, 0056 (absorbs 0057: headers); rule `server/security` | This is the best decision prose in the guide. Line 245 (the checklist for a new endpoint that takes an id) is pure rule. **Stale:** line 239 says the token holds "`jti`, `sub` and `tokenVersion` and nothing else", but `PotClaimTypes.cs` adds `session_id` (and the claim is `token_version`). Overlaps AUTHENTICATION.md. |
| Feature Implementation Recipe: Step-by-Step Guide | 249–267 | 0/30/70 | How-to | `docs/add-a-feature.md` | – |
| Example Walkthrough, Steps 1–13 | 268–889 | 10/20/70 | How-to | `docs/add-a-feature.md`; rules `server/endpoints`, `server/services` | All ten file paths I checked exist (AccountsEndpoints.cs, Create/RequestMapping.cs, EntityChecks/Checks/*, Concerns/OpenApi/SchemaReferenceId.cs …). The embedded "why" decisions go to: OpenAPI schema naming (324–331) → 0028; Request vs Input (433–437) → 0042; validator vs entity check (360–361) and Chain of Responsibility (762–767) → 0046. Its code is repeated almost verbatim in Endpoint Patterns and Service Layer: keep this as the one worked example. |
| Example Walkthrough, Step 14 (test + error bodies) | 890–967 | 45/35/20 | Decision | 0023 (absorbs 0026), 0025, 0022; rule `server/error-handling` | The 409 `type` URN table and the "which endpoints declare 409" policy (956–966) are contract decisions buried in a how-to step. They duplicate Problem Details 3851–3925. Move them out and leave a link. Port 5242 and `/scalar` are current. |
| Feature Organization (API/App/Data/Shared) | 968–1241 | 10/30/60 | Duplicate | 0041; rule `server/project-structure` | The four trees repeat Project Structure (65–147) and the walkthrough. The App "Service Pattern" code (1087–1123) repeats the Service Layer. Keep only the "Critical" WithTracking note (1204) as a `server/data-access` rule. Collapse the rest into one structure page. |
| Endpoint Patterns: Handler Pattern | 1242–1431 | 10/25/65 | How-to | 0042; rule `server/endpoints`; `docs/endpoints.md` | The step order (validate → map → call → typed result) is the rule. The six handler variants are a good catalogue. |
| Request Validation | 1432–1549 | 10/25/65 | How-to | 0046; rule `server/endpoints`; `docs/endpoints.md` | "Common Validation Rules" (1515–1548) is generic FluentValidation: trim it. The validator base-class rules overlap allsoverit-patterns. |
| Response Construction | 1550–1644 | 10/30/60 | How-to | 0042, 0021/0022 (ResponseBase); rule `server/endpoints` | Current, including the note that there is no zero-argument `Response.Ok()`. |
| Endpoint Registration | 1645–1762 | 25/40/35 | Rule | 0028 (response metadata), 0039 (group rate limit); rule `server/endpoints` | Lines 1697–1706 (Produces vs ProducesProblem, 400/500 on the group, 408/413 per route) are binding and pinned by `EndpointResponseMetadataFixture`. |
| Service Layer: Service Pattern | 1763–2039 | 5/25/70 | How-to | 0045, 0030; rule `server/services`; `docs/services-and-entity-checks.md` | **Rule conflict:** line 1874 says to guard every DI dependency with `WhenNotNull()` (as `CreateAccountService.cs:27` still does), but allsoverit-patterns:18 says not to. Lines 2010 (empty list, never 404) and 2037 (foreign reads as 404) are rules that duplicate Security. The ConfigureAwait note (1882) is C# style (plan §9). |
| Entity Checks | 2040–2226 | 15/20/65 | How-to | 0046; rule `server/services` | The code duplicates walkthrough Step 10 (623–767): keep one copy. The guidelines (2218–2225) are rule. |
| Result Pattern | 2227–2367 | 30/30/40 | Decision | 0045, 0025 ("no 422", 2357–2359), 0028 (2361–2366); rule `server/error-handling` | The rationale (2231–2237) is ADR material. The contract paragraph (2361–2366) duplicates the OpenAPI section of server.instructions. |
| Transaction Management | 2368–2473 | 15/35/50 | How-to | 0047; rule `server/data-access` | The guidelines (2438–2444) are rule. Merge in the "query, act, query, act" rule from server.instructions, which is missing here. |
| Repository Layer: Repository Pattern | 2474–2721 | 20/30/50 | How-to | 0047, 0030 (no `IQueryable` for User/Site); rule `server/data-access`; `docs/data-access.md` | The method-naming list (2706–2720) is rule. |
| Tracking vs No-Tracking | 2722–2803 | 30/30/40 | Duplicate | 0047; rule `server/data-access` | Repeats 2581–2605, and Performance Considerations repeats it again (4237–4255). Keep one copy. |
| Query Specifications | 2804–2918 | 10/20/70 | How-to | 0043 (AllOverIt) / 0047; `docs/data-access.md` | No ADR of its own. |
| Mapping Patterns (Request→Input, Entity→Output) | 2919–3086 | 20/35/45 | Rule | 0042 (no AutoMapper); rules `server/project-structure` (placement), `server/endpoints` | The `RequestMapping` placement duplicates server.instructions ("Feature-specific request mapping …"). |
| Concerns (intro) | 3087–3109 | 0/20/80 | How-to | `docs/cross-cutting.md` | A folder tree only. |
| Authentication & Authorization | 3110–3266 | 30/20/50 | Stale | 0031, 0032, 0033, 0037, 0038; rule `server/security`; `Docs/reference/auth-and-sessions.md` | **Stale:** the claims block (3248–3258) omits `session_id` (plan §10.8). "Refresh Token Pattern" (3260–3265) is thin and duplicates AUTHENTICATION.md, so replace it with a link. The permission list (3241–3246) belongs in `reference/permissions.md`. |
| Validation (Concerns) | 3267–3412 | 15/25/60 | How-to | 0046; `docs/cross-cutting.md` | Internals of `ProblemDetailsInspector`, for maintainers. Overlaps Request Validation (1432–1549). |
| CORS Configuration (first copy) | 3413–3501 | 40/30/30 | Decision | 0037 (absorbs 0040); `Docs/reference/configuration.md` | Keep this copy. The exposed-headers rationale and the 10-minute preflight cache (3469) are ADR text. Line 3481 (the `AllowedOrigins` format) is reference. |
| Rate Limiting | 3502–3632 | 40/20/40 | Decision | 0039, 0072 (`Retry-After`); rule `server/security` | Defaults 15 and 50 match `RateLimiterDefaults.cs`. Per-IP partitioning (`RateLimiterPolicy.cs:32`) is 0039's "Ver" item. The numbers belong in reference, not prose. |
| Middleware | 3633–3822 | 50/25/25 | Decision | 0056 (absorbs 0057; request logging metadata only), 0023 (absorbs 0027; correlation ID); rules `server/security`, `server/error-handling` | The documented order matches `Program.cs:52–110` (checked). The rationale prose (3690–3729) is ADR material. The UserContext/ICurrentUserContext code (3731–3822) is How-to. |
| Problem Details | 3823–4048 | 45/30/25 | Decision | 0023 (absorbs 0026/0027), 0025, 0056; rule `server/error-handling`; default-`detail` table → `Docs/reference/` | **Stale:** the `DatabaseExceptionHandler` sample (3983–4045) differs from the real handler, which maps to fixed 409/500 messages (`DatabaseExceptionHandler.cs:26–33`; plan §10.1, fix early). The "always write" rationale (3901–3906) is ADR material. |
| Entity Framework Core (intro: EntityBase) | 4049–4078 | 50/40/10 | Decision | 0021, 0022 | "Why three identifiers" is ADR text. Duplicated at 4285–4310. |
| Entity / Table Naming Convention | 4079–4156 | 30/50/20 | Rule | 0048; rule `server/project-structure` (naming) | The debug-only suffix check exists (`DbContextBase.cs:16, 90`). |
| Entity Conventions + Entity Configuration | 4157–4234 | 10/70/20 | Rule | 0048; rule `server/data-access` | The data annotations claim is true (`AccountEntity.cs:10`). "Seal entity classes" overlaps server.instructions. |
| Performance Considerations | 4235–4284 | 20/50/30 | Duplicate | 0047; rule `server/data-access` | Repeats Tracking (2722–2803) and server.instructions (N+1, no-tracking). |
| API Design Guidelines | 4285–4310 | 30/60/10 | Duplicate | 0021, 0022 | The third statement of RowId/Etag (after 203–205 and 4066–4072). Delete. |
| Enriched Enums | 4311–4426 | 35/25/40 | Decision | 0048; rule `server/data-access`; `docs/entities-and-schema.md` | **Stale:** the `Frequency` sample (4323) omits `EndOfMonth` (`Frequency.cs:14`). "Why strings" (4352–4357) is ADR text. The enum list (4412–4423) is reference, and its permission list matches `Permission.cs`. |
| Entity Relationships | 4427–4553 | 20/30/50 | How-to | 0049 (cascade restrict); `docs/entities-and-schema.md` | The cascade rationale (4548–4550) belongs in the ADR. |
| Indexes | 4554–4638 | 15/40/45 | How-to | 0048 (citext + check-plus-index, 4570–4572); rule `server/data-access` | Current. |
| Multi-Tenancy & Query Filters | 4639–4743 | 45/30/25 | Decision | 0030; rules `server/security`, `server/data-access` | The third statement of the two-mechanism model (after 156/182–186 and 236). Keep the mechanics here and move the "why" to 0030. |
| Optimistic Concurrency (ETags) | 4744–4826 | 40/20/40 | Decision | 0022 (Ver) | "Why" (4818–4823) is ADR text. The plan §11 caveat applies: this is not a database concurrency token. |
| Migrations | 4827–4949 | 15/25/60 | How-to | 0050; rule `server/data-access`; `docs/migrations.md` | **Stale:** line 4898 says the runner is used by "CI/CD pipelines", but there is no CI (0090). The commands are repeated at 5277–5285 and in Docker 189–193. The "humans generate migrations" rule (server.instructions) is missing here. |
| CORS Configuration (second copy) | 4950–5025 | 40/30/30 | Duplicate | Delete (plan §6.3); move 5022 (startup validation) → 0060 (absorbs 0052) | Keep only line 5022. |
| Dependency Injection | 5026–5226 | 30/30/40 | Decision | 0044 (Dec); rule `server/di-and-configuration`; `docs/dependency-injection.md` | **Conflict:** the guide and the code agree (91 Pot.App files implement `IPotScopedDependency`; `AutoRegisterScoped` in WebApplicationBuilderExtensions.cs:447), while server.instructions (DI section) says "register concrete classes … interface only when …". The instruction file is the outlier. |
| Accrual Metrics Semantics | 5227–5266 | 90/10/0 | Decision (domain) | 0010, 0011, 0012; `Docs/explanation/cashflow-model.md` | Domain explanation, not developer guidance. Duplicates ARCHITECTURE 120–133 and IMPLEMENTATION. Remove from this guide. |
| Available Commands | 5267–5297 | 0/10/90 | How-to | The index, or `docs/commands.md` | Migration rows repeat 4849–4939. |
| Additional Resources | 5298–5308 | – | How-to | The index | – |

### E.2 Client guide: `Source/Client/pot-react/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + Table of Contents | 1–45 | – | Delete | The new index | It lists headings that do not exist (Accounts/Expenses/Incomes Page, Reusable Layout Components) and omits Badge Styling System (plan §10.10). |
| Architecture Overview (stack + tree) | 46–101 | 10/10/80 | Stale | 0061; rule `client/project-structure` | Stale tree (plan §10.10). The "Vite 8" version belongs in package.json (plan §6.2). Duplicates ARCHITECTURE 81–93. |
| Project Organization | 102–261 | 35/45/20 | Rule | 0061; rule `client/project-structure` | "Why this pattern" (201–207) and lib vs concerns (191–199) are ADR text. **Stale:** line 260 says "Features never import from other features", but accounts, expenses, incomes, users and userSettings import `@/features/auth`, and dashboard imports expenses and incomes (grep). That is 0061's "Ver". The feature tree (114–124) omits the real `create/ edit/ delete/ schemas/ utils/` sub-folders. |
| TypeScript Standards | 262–323 | 25/25/50 | Tool-enforced (Stale) | 0076; config `eslint.config.js`, `tsconfig.app.json` | **Stale:** it cites `tseslint.configs.strict` in `eslint.config.mjs`. The real file is `eslint.config.js`, using `recommended` + `stylistic` (lines 14–16). `no-non-null-assertion` and `no-floating-promises` are not enabled. Reduce to "see the config" plus the "no `any`" rule. |
| Code Formatting (exports, barrels, spacing, braces) | 324–477 | 10/60/30 | Rule | rule `client/code-style`; config `.prettierrc` | None of these is lint-enforced (no `curly` or padding rules in eslint.config.js), so they really are rules. They duplicate client.instructions "TypeScript style". The import sort is enforced by `simple-import-sort` and should only be pointed at. |
| Function Declarations | 478–518 | 30/50/20 | Rule | rule `client/code-style` | Duplicates client.instructions:38–39. |
| Custom Components (intro) | 519–528 | 30/70/0 | Rule | 0062 | Duplicates client.instructions:16. |
| Table, Cards, Dialog components | 529–719 | 5/10/85 | How-to | `docs/components.md` | A catalogue (plan §9). |
| Feedback components | 720–793 | 5/25/70 | How-to | `docs/components.md`; 0064 (absorbs 0065) | **Stale:** line 765 imports `@/hooks/useToast`, which does not exist. The guidelines (782–788) duplicate 2223–2240 and client.instructions:30. |
| Badge Styling System | 794–1023 | 10/40/50 | Rule | rule `client/components-and-styling`; `docs/styling-and-layout.md` | The colour-semantics table and "always use the helpers" are rules. Duplicates client.instructions:31. `src/lib/badgeStyles.ts` exists. |
| Filter, Input, Layout, Navigation, Picker, Theme, User components | 1024–1353 | 5/10/85 | How-to | `docs/components.md` | Theme persistence (1284–1285) is a storage policy → 0067. |
| Sheet Components | 1354–1595 | 20/45/35 | Rule | 0069; rule `client/components-and-styling` | The binding conventions are `modal={false}`, hidden default X, the two widths and route-driven forms. The Separator + `opacity-80` rule (client.instructions:31) is missing here. |
| Projections Page | 1596–1785 | 5/25/70 | Stale | `docs/styling-and-layout.md` | **Stale:** `ProjectionsPage.tsx:247` is `h-screen overflow-hidden`, not `md:h-screen`, and the chart is `h-[500px]`/`min-h-[500px]` with a short-viewport branch (`ProjectionChart.tsx:176–181`), not 400px. The "Pure CSS responsive" learning (1749–1753) is contradicted by that JS branch. Keep the learnings and drop the class-by-class breakdown. |
| Dashboard + Table-Based Pages | 1786–1869 | 5/40/55 | Stale | 0068, 0067; `docs/styling-and-layout.md` | **Stale:** 1854–1866 says filters persist in localStorage, but `useExpenseStorage.ts:34` uses sessionStorage and the URL (plan §10.10). |
| Error Handling: core, standard pattern, examples, mutation, storage, early return | 1870–2222 | 5/25/70 | How-to | 0064 (absorbs 0065); `docs/error-handling.md` | The root error boundary (0065) is not documented anywhere in the guide. |
| When to Use ErrorSheet vs Toasts + Guidelines | 2223–2254 | 20/80/0 | Rule | 0064; rule `client/error-handling` | The core rule list. Duplicates 782–788 and client.instructions:30. |
| Common Patterns | 2255–2322 | 0/10/90 | Duplicate | – | Repeats 1954–2221. Delete. |
| State Management | 2323–2384 | 30/50/20 | Duplicate | 0063 (absorbs 0066), 0067; rule `client/data-and-state` | Duplicates client.instructions "State and data flow" and ARCHITECTURE 269–274. The `useLocalStorage('theme')` example ignores the env/user-scoped keys (0067). React Hook Form + Zod (0070) is not covered anywhere in the guide. |
| Authentication & Authorization | 2385–2544 | 15/35/50 | Stale | 0071, 0032; rule `client/auth-and-routing` | **Stale paths:** AuthContext is at `src/features/auth/contexts/AuthContext.tsx`, and logoutManager is at `src/concerns/auth/logoutManager.ts` (2391, 2422, 2427). "PermissionGuard vs WithPermission" (2506–2509) is a rule. The permission list belongs in `reference/permissions.md`. |
| Routing | 2545–2611 | 30/40/30 | Rule | 0069; rule `client/auth-and-routing` | **Stale:** a `react-router-dom` import (2556). The `/logout` and catch-all rules duplicate client.instructions:27. |
| API Integration: Result Pattern | 2612–2653 | 40/30/30 | Decision | 0064 | – |
| Available API Hooks | 2654–2673 | 0/20/80 | How-to | `docs/data-fetching.md` | The names match the `useApi.ts` exports (checked). |
| Cache Invalidation | 2674–2706 | 20/40/40 | Stale | 0063; rule `client/data-and-state` | **Stale:** the path `src/lib/cacheInvalidation.ts` should be `src/concerns/cache/cacheInvalidation.ts`. |
| Error Types | 2707–2726 | 10/10/80 | Stale | 0064, 0025 | Lists `ValidationError (422)`, which does not exist. `apiErrors.ts` has `BadRequestError` and `ServerError`, and the list omits both. Replace the list with a pointer to the file. |
| Import/Export Features | 2727–2766 | 50/20/30 | Decision (local) | 0063; `docs/data-fetching.md` | A small design note, not ADR-worthy on its own. |
| Environment Configuration | 2767–2836 | 20/30/50 | How-to | 0085 (absorbs 0073); `Docs/reference/configuration.md` | **Stale:** `.env.development` and `.env.production` are git-ignored (`.gitignore:405–406`) and absent from a clean checkout, yet they are shown as if tracked. Duplicates Docker 490–502. |
| Testing Mocking Strategy | 2837–2915 | 60/25/15 | Decision | 0078; rule `client/testing` | Already written almost as an ADR (options, trade-offs, triggers). The rule side is in client-tests.instructions. |
| Available Commands | 2916–2938 | 0/10/90 | How-to | The index | **Stale:** "ESLint with strict TypeScript rules". It omits the `e2e:*` scripts; point at package.json instead. |
| PWA: What is a PWA? + Why PWA | 2939–2964 | 60/0/40 | Decision | 0074 | "What is a PWA" is generic: delete it. "Why" is ADR context. |
| PWA: Setup, `serialize-javascript` override, Icon Generation | 2965–3028 | 30/10/60 | How-to | 0077 or the new candidate N1; `docs/pwa.md` | The `vite-plugin-pwa@^1.3.0` pin duplicates package.json. The CVE override and the removal of the generator are decision records. |
| `vite.config.ts` Changes | 3029–3081 | 50/20/30 | Decision | 0074 | Restates config: point at `vite.config.ts`. The "Key configuration decisions" table is ADR content. |
| Service Worker Registration and Update UX | 3082–3125 | 55/20/25 | Decision | 0074 | The strongest decision content in this guide: enforced update, quiet window, `version.json` channel (the files exist under `src/concerns/pwa/`). **Conflict:** line 3122 relies on a "backward-compatibility policy", but 0020 says breaking changes need no compatibility window. |
| `index.html` / TypeScript support / Build Output | 3126–3177 | 5/10/85 | Delete | – | A changelog ("was updated"). The precache count "~64 entries" drifts. The files themselves are the source. |
| Validating PWA + Local Sanity Check | 3178–3215 | 0/10/90 | How-to | `docs/pwa.md` | – |
| Additional Resources | 3216–3228 | – | How-to | The index | – |

### E.3 Docker guide: `Source/Docker/DEVELOPER.md`

| Section | Lines | Mix (D/R/H) | Primary category | Target | Notes |
| --- | --- | --- | --- | --- | --- |
| Title + TOC | 1–26 | – | How-to | Keep | The `Docs/DOCKER-SETUP.md` link must follow the plan's rename. |
| Overview | 27–47 | 10/20/70 | How-to (reference) | 0088; `Docs/reference/ports-and-environments.md` | The port table duplicates docker.instructions:9 and Data/README. |
| Other Docker stacks | 48–60 | 30/40/30 | Rule | 0088; rule `docker` | "One stack at a time" is repeated at 813. |
| Key terms | 61–73 | – | How-to | Keep (or the glossary) | – |
| Dependencies and startup order | 74–92 | 30/20/50 | Decision | 0050, 0053 | Matches the compose file (checked). Only postgres and client have health checks, so 0053's readiness has no compose expression (see N2). |
| Ports | 93–109 | 10/20/70 | How-to (reference) | `Docs/reference/ports-and-environments.md`; 0084 (Ver) | **Contradiction:** "ingress routes `/api/*` to the API container" against `VITE_API_BASE_URL=https://api.payontime.com.au/api` (497, 687). That is exactly 0084's "Ver". The `Azure-Deployment-Notes.md` link (106) is dead. |
| Development Workflow | 110–200 | 5/25/70 | How-to | `Docs/how-to/run-the-docker-stack.md`; rule `docker` | **Stale:** 115–116 say "each developer creates their own" `.env.development`, but `Source/Docker/.env.development.template` exists (plan §10.12). The rules are "Do not add `-p pot`" (171) and the env-file pair (153–156). The migration command duplicates the server guide. |
| Docker Compose Configuration | 201–313 | 20/10/70 | How-to | 0087 (`no_cache`), 0088; keep | The excerpts match the compose file (checked). The client `environment: NODE_ENV` is omitted (minor). The network YAML is repeated at 510–514. |
| Images and Dockerfiles | 314–406 | 35/10/55 | How-to | 0085 (absorbs 0073: build-time config), 0050 (migrations in entrypoint, `set -e`), 0087; keep | `entrypoint.sh` matches. "Node 24 is the minimum" (364) conflicts with the setup docs (plan §10.12), and versions belong in the Dockerfile. **Risk:** the `.env.production` fallback (377–379) relies on a git-ignored local file, and `.dockerignore` also drops the client `.env`, so a clean clone has neither fallback. The nginx notes omit the `/version.json` no-cache location (`nginx.conf:25`). |
| Environment Variables: `.env`, `.env.development` | 407–464 | 30/20/50 | How-to | 0089; `Docs/reference/configuration.md` | Should say "copy `.env.development.template`". |
| Variables compose passes / Client build arguments | 465–505 | 10/10/80 | How-to (reference) | `Docs/reference/configuration.md` | Matches the compose file (checked). Duplicates client guide 2767–2836. |
| Networking | 506–542 | 10/40/50 | How-to | 0088; rule `docker` (use service names, not localhost) | – |
| Data Persistence + Backup | 543–591 | 20/20/60 | How-to | 0088 (bind mounts); `Docs/how-to/run-the-docker-stack.md` | The reset steps are repeated at 771–774. |
| Health Checks | 592–664 | 25/15/60 | How-to | 0053; see N2 | 633–647 propose adding a server health check. That is an open design choice, not documentation. |
| Production Deployment | 665–717 | 40/10/50 | Decision | 0084 (Ver), 0087, 0090; `Docs/how-to/deploy-to-azure.md` | The link at 714 is dead. It contains the same `/api` ingress contradiction. |
| Troubleshooting | 718–785 | 0/10/90 | How-to | Keep | "Rebuild after changing code" (776–785) duplicates 183–187. |
| Best Practices | 786–824 | 40/50/10 | Duplicate | 0085, 0089, 0088, 0053; rule `docker` | Generic advice that restates the earlier sections and the ADR rationale. "Pin base image versions" while using `nginx:alpine` and `postgres:13` is inconsistent. Delete once the ADRs exist; move the few real rules to `docker`. |
| Additional Resources | 825–840 | – | How-to | Keep | The link at 837 is dead. |


### E.4 Summary

#### E.4.1 Lines per category (approximate)

These totals are mix-weighted, so a section that is 30% rule adds 30% of its lines to Rule. Code counts as How-to.

| Guide | Total | Decision | Rule | How-to / example | Delete outright | Duplicate (primary) | Stale (primary) |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Server | 5,308 | ~1,200 (23%) | ~1,470 (28%) | ~2,580 (49%) | ~60 | ~510 | ~160 |
| Client | 3,228 | ~500 (16%) | ~920 (28%) | ~1,760 (55%) | ~100 | ~130 | ~540 |
| Docker | 840 | ~160 (19%) | ~140 (17%) | ~530 (64%) | 0 | ~40 | 0 |

**Tool-enforced:**

- Server: no section is tool-enforced. `.editorconfig` style lives only in server.instructions.
- Client: only TypeScript Standards (~60 lines), plus parts of Code Formatting.

**Duplicated or stale sections that are not primary:**

- The "Duplicate" and "Stale" columns count only sections where that is the primary category. Many more sections carry a duplicate or stale paragraph (see the Notes columns).
- Most of the server's decision lines are already stated two or three times inside the guide itself: tenancy three times, RowId/Etag three times, CORS twice, problem details twice.

#### E.4.2 Proposed rule documents

**Server (8 topics):**

| Topic | Fed by (lines) | Also merge |
| --- | --- | --- |
| `server/project-structure` (layers, slices, naming) | 61–147, 199–231, 968–1241 (trimmed), 2919–3086 (placement), 2706–2720, 4079–4156, 4865–4869 | server.instructions "Architecture" |
| `server/endpoints` (handler, validation call, response, registration, OpenAPI metadata) | 249–889 (rules only), 1242–1762, 2923–2993 | server.instructions "OpenAPI contract" |
| `server/services` (service shape, entity checks, results, transactions) | 1763–2226, 2227–2367 (rules), 2368–2473 | allsoverit-patterns |
| `server/error-handling` (status map, problem body, 409 types, logging of failures) | 199–231 (errors), 890–967, 2326–2366, 3633–3822 (correlation), 3823–4048 | – |
| `server/data-access` (repos, tracking, specs, entities, enums, relationships, indexes, ETags, migration guidelines) | 2474–2918, 4049–4638, 4744–4826, 4940–4949 | server.instructions "Persistence" |
| `server/security` (tenancy checklist, authz, CORS, rate limits, headers) | 232–248 (esp. 245), 3110–3266, 3413–3632, 3690–3704, 4639–4743 | – |
| `server/di-and-configuration` (registration, lifetimes, validated options) | 5026–5226, 5022, 3481 | Resolve 0044 first. |
| `server/testing` (fixtures that pin architecture rules) | Barely fed by the guide: only the fixture names it cites (`EndpointResponseMetadataFixture`, `OpenApiSecurityFixture`, `SchemaReferenceIdFixture`, `EnrichedErrorExtensionsFixture`) | dotnet-tests and aspnetcore-integration-tests instructions |

**Client (8 topics):**

| Topic | Fed by (lines) | Also merge |
| --- | --- | --- |
| `client/project-structure` | 46–261 | client.instructions "Architecture" |
| `client/code-style` (with pointers to eslint, prettier and tsconfig for the enforced parts) | 262–518 | client.instructions "TypeScript style" |
| `client/components-and-styling` (shadcn rule, sheets, badges, page-layout learnings) | 519–528, 794–1023, 1354–1595, 1747–1784, 1803–1858 | – |
| `client/error-handling` | 782–788, 2223–2254, rule lines of 1870–2222 | – |
| `client/data-and-state` (state split, Result, cache invalidation, storage scope, URL filters) | 2323–2384, 2612–2766, 1860–1866 (corrected) | – |
| `client/auth-and-routing` | 2385–2611 | – |
| `client/testing` | 2837–2915 | client-tests.instructions |
| `client/build-and-pwa` (env vars baked at build, PWA never caches `/api`, enforced update) | 2767–2836, 2939–3125 | – |

**Docker (1 topic):** `docker`, fed by lines 48–60, 153–156, 171, 506–542 and the rule lines of 786–824. Merge docker.instructions, which already holds most of it.

#### E.4.3 New ADR candidates not in the backlog

I found few. Most of the guides' decision content maps to existing rows.

- **N1 — Handle dependency advisories by override or temporary install, and commit generated artefacts.**
  - Evidence: client 2975 and 2979–3011. The `serialize-javascript` override was applied, and `@vite-pwa/assets-generator` was removed because of `sharp` CVEs, with the icons committed instead.
  - Could fold into 0077 (the dependency upgrade policy).
- **N2 — The prodlike compose stack has no server health check, and the client depends only on the container starting.**
  - Evidence:
    - Docker 87–91, 296–298 and 619–647.
    - The compose file defines health checks only on postgres and client.
    - The guide openly proposes adding one.
  - It is a live "Dec" that 0053 (liveness/readiness) does not cover for compose. Fold into 0053 or 0088 as a decision point if you prefer.
- **N3 — The OpenAPI document declares only the failures an endpoint's own rules can produce.**
  - Evidence: server 518–520, 964–966 and 1697–1706, plus the schema names per slice at 324–331, pinned by fixtures.
  - This is a real, reasoned decision ("documenting 409 on every write would present a conflict as normal"). I recommend widening 0028's scope rather than adding a row.

There are also two contradictions to resolve while writing the ADRs. They are not new ADRs:

- Client 3122 assumes a backward-compatibility policy, which contradicts 0020.
- The server DI section contradicts server.instructions (0044).

#### E.4.4 Sections to delete outright

| Guide | Lines | Section | Why |
| --- | --- | --- | --- |
| Server | 1–60 | TOC | Wrong; replaced by the split index. |
| Server | 968–1241 | Feature Organization | Repeats Project Structure and the walkthrough. Keep the one WithTracking warning. |
| Server | 4235–4284 | Performance Considerations | Repeats Tracking and server.instructions. |
| Server | 4285–4310 | API Design Guidelines | The third copy of RowId/Etag. |
| Server | 4950–5025 | CORS Configuration, second copy | The plan already says to delete it. Keep line 5022 (to 0060). |
| Server | 5227–5266 | Accrual Metrics Semantics | Move to `explanation/cashflow-model.md` and 0010–0012; it is domain content, not developer guidance. |
| Server | 3260–3265 | Refresh Token Pattern | Thin and partly stale. Replace with a link to `reference/auth-and-sessions.md`. |
| Client | 1–45 | TOC | Stale. |
| Client | 2255–2322 | Common Patterns | Repeats the error examples above it. |
| Client | 2941–2951 | What is a PWA? | Generic web content. |
| Client | 3126–3177 | `index.html` / TS support / Build Output | A changelog of a past change, not guidance. |
| Client | 1606–1745 | Class-by-class Projections layout breakdown | Stale. Keep only the learnings. |
| Client | 2707–2726 | Error Types | Stale. Replace with a pointer to `src/api/errors/apiErrors.ts`. |
| Docker | 786–824 | Best Practices | Restates the guide and the future ADR rationale. Move its few rules to `docker`. |
| Docker | 776–785 | Rebuild after changing code | A verbatim duplicate of 183–187. |

#### E.4.5 Contradictions that need a decision

These four came out of the classification. Each has a brief in `DECISIONS-TO-DISCUSS.md` (D10), along with candidate N2 (no server health check in the compose stack):

- **Feature-to-feature imports.** Client line 260 says features never import other features, but six feature folders do (`auth`, `expenses`, `incomes`). This is row 0061's `Ver` item.
- **DI registration.** The server guide and the code agree on marker-interface scanning; `server.instructions.md` is the outlier. This is row 0044.
- **Backward compatibility.** Client line 3122 relies on a backward-compatibility policy, which conflicts with row 0020 (no compatibility window).
- **`WhenNotNull` on injected dependencies.** Server line 1874, and the code (`CreateAccountService.cs:27`), guard every dependency, but `allsoverit-patterns.instructions.md:18` says not to.

> **Your call on Appendix E:** (a) the folded-row hosts in the table at the top; (b) the delete list in E.4.4; (c) any section you would classify differently.
